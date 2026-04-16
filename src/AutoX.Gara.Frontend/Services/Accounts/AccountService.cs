// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Models.Results.Accounts;
using AutoX.Gara.Frontend.Results.Accounts;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Auth;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Framework.DataFrames.SignalFrames;

namespace AutoX.Gara.Frontend.Services.Accounts;

/// <summary>
/// Implementation th?c t?: k?t n?i ? handshake ? g?i LOGIN packet ? d?i ph?n h?i.
/// To�n b? network I/O n?m ? d�y, ViewModel kh�ng bi?t g� v? TcpSession.
/// </summary>
public sealed class AccountService : IAccountService
{
    // --- C?u h�nh ------------------------------------------------------------

    private const System.Int32 ServerPort = 57206;
    private const System.String ServerHost = "127.0.0.1";

    private const System.Int32 LoginTimeoutMs = 5_000;
    private const System.Int32 HandshakeTimeoutMs = 5_000;

    // --- Logger (lazy + GetOrCreate d? an to�n) ------------------------------
    private ILogger Logger => InstanceManager.Instance.GetOrCreateInstance<ILogger>();

    // --- ConnectAsync ---------------------------------------------------------
    public async System.Threading.Tasks.Task<ConnectionResult> ConnectAsync(System.Threading.CancellationToken ct = default)
    {
        try
        {
            Logger.Debug($"[AccountService] ConnectAsync started ? {ServerHost}:{ServerPort}");

            TcpSession client = InstanceManager.Instance.GetOrCreateInstance<TcpSession>();

            await client.ConnectAsync(ServerHost, ServerPort, ct);
            Logger.Debug("[AccountService] TCP connection established successfully.");

            Logger.Debug($"[AccountService] Starting HandshakeAsync (opCode = {(System.UInt16)OpCommand.HANDSHAKE}, timeout = {HandshakeTimeoutMs}ms)");

            await client.HandshakeAsync(ct);
            System.Boolean ok = true;

            Logger.Debug($"[AccountService] HandshakeAsync completed ? Success = {ok}");

            if (ok)
            {
                Logger.Info("[AccountService] Handshake successful ? encryption channel ready.");
                return ConnectionResult.Success();
            }
            else
            {
                Logger.Warn("[AccountService] Handshake failed (check SDK.HandshakeAsync log for SocketException 10054 or other details).");
                return ConnectionResult.Failure("Handshake th?t b?i, kh�ng thi?t l?p du?c k�nh m� h�a.");
            }
        }
        catch (System.Exception ex)
        {
            Logger.Error($"[AccountService] ConnectAsync failed with exception: {ex}");
            if (ex.InnerException != null)
            {
                Logger.Error($"InnerException: {ex.InnerException}");
            }

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
            Logger.Debug($"[AccountService] AuthenticateAsync started for user: {username} (password masked)");

            TcpSession client = InstanceManager.Instance.GetOrCreateInstance<TcpSession>();

            // 1. Build packet
            LoginPacket packet = new();
            System.UInt32 sq = Csprng.NextUInt32();
            LoginRequestModel model = new() { Username = username, Password = password };
            packet.SequenceId = sq;
            packet.Initialize((System.UInt16)OpCommand.LOGIN, model);

            Logger.Debug($"[AccountService] LoginPacket built ? SequenceId = {sq}, OpCode = {(System.UInt16)OpCommand.LOGIN}");

            // 2. TaskCompletionSource + OnOnce
            System.Threading.Tasks.TaskCompletionSource<LoginResult> tcs = new(
                System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            sub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();

                    Logger.Debug($"[AccountService] Received Directive response (seq {sq}) ? Type = {resp.Type}, Reason = {resp.Reason}");

                    LoginResult result = resp.Type == ControlType.NONE
                        ? LoginResult.Success()
                        : MapErrorResponse(resp.Reason, resp.Action);

                    tcs.TrySetResult(result);
                });

            // 3. G?i packet
            Logger.Debug("[AccountService] Sending encrypted LoginPacket...");
            await client.SendAsync(packet, ct);
            Logger.Debug("[AccountService] LoginPacket sent ? waiting for response...");

            // 4. �?i k?t qu? v?i timeout
            using System.Threading.CancellationTokenSource cts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);
            System.Threading.Tasks.Task timeoutTask = System.Threading.Tasks.Task.Delay(LoginTimeoutMs, cts.Token);
            System.Threading.Tasks.Task winner = await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask);

            if (winner != tcs.Task)
            {
                sub?.Dispose();
                Logger.Warn($"[AccountService] Login TIMEOUT after {LoginTimeoutMs}ms");
                return LoginResult.Timeout();
            }

            LoginResult finalResult = await tcs.Task;
            Logger.Debug($"[AccountService] AuthenticateAsync completed ? {(finalResult.IsSuccess ? "SUCCESS" : "FAILURE")}");
            return finalResult;
        }
        catch (System.OperationCanceledException)
        {
            Logger.Debug("[AccountService] AuthenticateAsync was canceled by user.");
            return LoginResult.Failure("�ang nh?p b? h?y.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            Logger.Error($"[AccountService] AuthenticateAsync exception: {ex}");
            if (ex.InnerException != null)
            {
                Logger.Error($"InnerException: {ex.InnerException}");
            }

            return LoginResult.Failure($"L?i kh�ng x�c d?nh: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    // --- Error mapping --------------------------------------------------------

    private static LoginResult MapErrorResponse(ProtocolReason reason, ProtocolAdvice advice)
    {
        System.String message = reason switch
        {
            ProtocolReason.NOT_FOUND => "T�i kho?n kh�ng t?n t?i.",
            ProtocolReason.MALFORMED_PACKET => "G�i tin kh�ng h?p l?.",
            ProtocolReason.INTERNAL_ERROR => "L?i h? th?ng, vui l�ng th? l?i sau.",
            ProtocolReason.UNAUTHENTICATED => "Sai m?t kh?u, vui l�ng ki?m tra l?i.",
            ProtocolReason.FORBIDDEN => "T�i kho?n b? c?m ho?c chua du?c k�ch ho?t, vui l�ng li�n h? qu?n tr? vi�n.",
            ProtocolReason.ACCOUNT_LOCKED => "T�i kho?n t?m b? kh�a do nh?p sai nhi?u l?n, vui l�ng th? l?i sau 15 ph�t.",
            _ => "�ang nh?p th?t b?i, vui l�ng th? l?i."
        };

        return LoginResult.Failure(message, advice);
    }
}
