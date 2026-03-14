// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Results.Accounts;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Auth;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Frames.Controls;

namespace AutoX.Gara.Frontend.Services.Accounts;

/// <summary>
/// Implementation th?c t?: k?t n?i ? handshake ? g?i LOGIN packet ? d?i ph?n h?i.
/// Toàn b? network I/O n?m ? dây, ViewModel không bi?t gì v? ReliableClient.
/// </summary>
public sealed class AccountService : IAccountService
{
    // --- C?u hình ------------------------------------------------------------

    private const System.Int32 ServerPort = 57206;
    private const System.String ServerHost = "127.0.0.1";

    private const System.Int32 LoginTimeoutMs = 5_000;
    private const System.Int32 HandshakeTimeoutMs = 5_000;

    // --- ConnectAsync ---------------------------------------------------------

    public async System.Threading.Tasks.Task<ConnectionResult> ConnectAsync(System.Threading.CancellationToken ct = default)
    {
        try
        {
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            await client.ConnectAsync(ServerHost, ServerPort, ct);

            System.Boolean ok = await client.HandshakeAsync((System.UInt16)OpCommand.HANDSHAKE, timeoutMs: HandshakeTimeoutMs, ct: ct);

            return ok
                ? ConnectionResult.Success()
                : ConnectionResult.Failure("Handshake th?t b?i, không thi?t l?p du?c kênh mã hóa.");
        }
        catch (System.Exception ex)
        {
            return ConnectionResult.Failure(ex.Message);
        }
    }

    // --- AuthenticateAsync ----------------------------------------------------

    public async System.Threading.Tasks.Task<LoginResult> AuthenticateAsync(
        System.String username,
        System.String password,
        System.Threading.CancellationToken ct = default)
    {
        try
        {
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            // 1. Build packet
            LoginPacket packet = new();
            System.UInt32 sq = Csprng.NextUInt32();
            LoginRequestModel model = new() { Username = username, Password = password };

            packet.SequenceId = sq;
            packet.Initialize((System.UInt16)OpCommand.LOGIN, model);
            LoginPacket.Encrypt(packet, client.Options.EncryptionKey, CipherSuiteType.SALSA20);

            // 2. TaskCompletionSource d? "await" callback m?t l?n
            //    Dùng thay Task.Delay polling — không có race condition
            System.Threading.Tasks.TaskCompletionSource<LoginResult> tcs = new(
                System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            sub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();

                    LoginResult result = resp.Type == ControlType.NONE
                        ? LoginResult.Success()
                        : MapErrorResponse(resp.Reason, resp.Action);

                    tcs.TrySetResult(result);
                });

            // 3. G?i packet
            await client.SendAsync(packet, ct);

            // 4. Ð?i k?t qu? v?i timeout + cancellation
            using System.Threading.CancellationTokenSource cts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);

            System.Threading.Tasks.Task timeoutTask = System.Threading.Tasks.Task.Delay(LoginTimeoutMs, cts.Token);
            System.Threading.Tasks.Task winner = await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask);

            if (winner != tcs.Task)
            {
                sub?.Dispose();
                return LoginResult.Timeout();
            }

            return await tcs.Task;
        }
        catch (System.OperationCanceledException)
        {
            return LoginResult.Failure("Ðang nh?p b? h?y.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            InstanceManager.Instance.GetOrCreateInstance<ILogger>().Error(ex.ToString());
            if (ex.InnerException != null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ILogger>().Error("Inner: " + ex.InnerException);
            }

            return LoginResult.Failure($"L?i không xác d?nh: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    // --- Error mapping --------------------------------------------------------

    private static LoginResult MapErrorResponse(ProtocolReason reason, ProtocolAdvice advice)
    {
        System.String message = reason switch
        {
            ProtocolReason.NOT_FOUND => "Tài kho?n không t?n t?i.",
            ProtocolReason.MALFORMED_PACKET => "Gói tin không h?p l?.",
            ProtocolReason.INTERNAL_ERROR => "L?i h? th?ng. Vui lòng th? l?i sau.",
            ProtocolReason.UNAUTHENTICATED => "Sai m?t kh?u. Vui lòng ki?m tra l?i.",
            ProtocolReason.FORBIDDEN => "Tài kho?n b? c?m ho?c chua du?c kích ho?t. Vui lòng liên h? qu?n tr? viên.",
            ProtocolReason.ACCOUNT_LOCKED => "Tài kho?n t?m b? khóa do nh?p sai nhi?u l?n. Vui lòng th? l?i sau 15 phút.",
            _ => "Ðang nh?p th?t b?i. Vui lòng th? l?i."
        };

        return LoginResult.Failure(message, advice);
    }
}
