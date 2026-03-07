// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Packets;
using AutoX.Gara.UI.Services;
using Nalix.Common.Messaging.Protocols;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Messaging.Controls;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend.Services;

/// <summary>
/// Implementation thực tế: kết nối → handshake → gửi LOGIN packet → đợi phản hồi.
/// Toàn bộ network I/O nằm ở đây, ViewModel không biết gì về ReliableClient.
/// </summary>
public sealed class LoginService : ILoginService
{
    // ─── Cấu hình ────────────────────────────────────────────────────────────

    private const System.Int32 ServerPort = 57206;
    private const System.String ServerHost = "127.0.0.1";

    private const System.Int32 LoginTimeoutMs = 5_000;
    private const System.Int32 HandshakeTimeoutMs = 5_000;

    // ─── ConnectAsync ─────────────────────────────────────────────────────────

    public async Task<ConnectionResult> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            await client.ConnectAsync(ServerHost, ServerPort, ct);

            System.Boolean ok = await client.HandshakeAsync((System.UInt16)OpCommand.HANDSHAKE, timeoutMs: HandshakeTimeoutMs, ct: ct);

            return ok
                ? ConnectionResult.Success()
                : ConnectionResult.Failure("Handshake thất bại, không thiết lập được kênh mã hóa.");
        }
        catch (System.Exception ex)
        {
            return ConnectionResult.Failure(ex.Message);
        }
    }

    // ─── AuthenticateAsync ────────────────────────────────────────────────────

    public async Task<LoginResult> AuthenticateAsync(
        System.String username,
        System.String password,
        CancellationToken ct = default)
    {
        try
        {
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            // 1. Build packet
            System.UInt32 sq = Csprng.NextUInt32();
            AccountPacket packet = new();
            AccountModel model = new() { Username = username, Password = password };

            packet.SequenceId = sq;
            packet.Initialize((System.UInt16)OpCommand.LOGIN, model);

            // 2. TaskCompletionSource để "await" callback một lần
            //    Dùng thay Task.Delay polling — không có race condition
            TaskCompletionSource<LoginResult> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

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

            // 3. Gửi packet
            await client.SendAsync(packet, ct);

            // 4. Đợi kết quả với timeout + cancellation
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            Task timeoutTask = Task.Delay(LoginTimeoutMs, cts.Token);
            Task winner = await Task.WhenAny(tcs.Task, timeoutTask);

            if (winner != tcs.Task)
            {
                sub?.Dispose();
                return LoginResult.Timeout();
            }

            return await tcs.Task;
        }
        catch (System.OperationCanceledException)
        {
            return LoginResult.Failure("Đăng nhập bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            return LoginResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    // ─── Error mapping ────────────────────────────────────────────────────────

    private static LoginResult MapErrorResponse(ProtocolReason reason, ProtocolAdvice advice)
    {
        System.String message = reason switch
        {
            ProtocolReason.MALFORMED_PACKET => "Gói tin không hợp lệ.",
            ProtocolReason.NOT_FOUND => "Tài khoản không tồn tại.",
            ProtocolReason.ACCOUNT_LOCKED => "Tài khoản tạm bị khóa do nhập sai nhiều lần. Vui lòng thử lại sau 15 phút.",
            ProtocolReason.UNAUTHENTICATED => "Sai mật khẩu. Vui lòng kiểm tra lại.",
            ProtocolReason.FORBIDDEN => "Tài khoản bị cấm hoặc chưa được kích hoạt. Vui lòng liên hệ quản trị viên.",
            ProtocolReason.INTERNAL_ERROR => "Lỗi hệ thống. Vui lòng thử lại sau.",
            _ => "Đăng nhập thất bại. Vui lòng thử lại."
        };

        return LoginResult.Failure(message, advice);
    }
}