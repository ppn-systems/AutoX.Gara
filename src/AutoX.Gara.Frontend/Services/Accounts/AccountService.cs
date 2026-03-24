// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Models.Results.Accounts;
using AutoX.Gara.Frontend.Results.Accounts;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Auth;
using Nalix.Common.Diagnostics;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Frames.Controls;

namespace AutoX.Gara.Frontend.Services.Accounts;

/// <summary>
/// Implementation th?c t?: k?t n?i ? handshake ? g?i LOGIN packet ? đổi phụn h?i.
/// Toàn b? network I/O n?m ? dây, ViewModel không bi?t gì vụ TcpSession.
/// </summary>
public sealed class AccountService : IAccountService
{
    // --- C?u hình ------------------------------------------------------------

    private const System.Int32 ServerPort = 57206;
    private const System.String ServerHost = "127.0.0.1";

    private const System.Int32 LoginTimeoutMs = 5_000;
    private const System.Int32 HandshakeTimeoutMs = 5_000;

    // --- Logger (lazy + GetOrCreate để an toàn) ------------------------------
    private ILogger Logger => InstanceManager.Instance.GetOrCreateInstance<ILogger>();

    // --- ConnectAsync ---------------------------------------------------------
    public async System.Threading.Tasks.Task<ConnectionResult> ConnectAsync(System.Threading.CancellationToken ct = default)
    {
        try
        {
            Logger.Debug($"[AccountService] ConnectAsync started → {ServerHost}:{ServerPort}");

            TcpSession client = InstanceManager.Instance.GetOrCreateInstance<TcpSession>();

            await client.ConnectAsync(ServerHost, ServerPort, ct);
            Logger.Debug("[AccountService] TCP connection established successfully.");

            Logger.Debug($"[AccountService] Starting HandshakeAsync (opCode = {(System.UInt16)OpCommand.HANDSHAKE}, timeout = {HandshakeTimeoutMs}ms)");

            System.Boolean ok = await client.HandshakeAsync(
                (System.UInt16)OpCommand.HANDSHAKE,
                timeoutMs: HandshakeTimeoutMs,
                ct: ct);

            Logger.Debug($"[AccountService] HandshakeAsync completed → Success = {ok}");

            if (ok)
            {
                Logger.Info("[AccountService] Handshake successful → encryption channel ready.");
                return ConnectionResult.Success();
            }
            else
            {
                Logger.Warn("[AccountService] Handshake failed (check SDK.HandshakeAsync log for SocketException 10054 or other details).");
                return ConnectionResult.Failure("Handshake thất bại, không thiết lập được kênh mã hóa.");
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

            Logger.Debug($"[AccountService] LoginPacket built → SequenceId = {sq}, OpCode = {(System.UInt16)OpCommand.LOGIN}");

            // 2. TaskCompletionSource + OnOnce
            System.Threading.Tasks.TaskCompletionSource<LoginResult> tcs = new(
                System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            sub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();

                    Logger.Debug($"[AccountService] Received Directive response (seq {sq}) → Type = {resp.Type}, Reason = {resp.Reason}");

                    LoginResult result = resp.Type == ControlType.NONE
                        ? LoginResult.Success()
                        : MapErrorResponse(resp.Reason, resp.Action);

                    tcs.TrySetResult(result);
                });

            // 3. Gửi packet
            Logger.Debug("[AccountService] Sending encrypted LoginPacket...");
            await client.SendAsync(packet, ct);
            Logger.Debug("[AccountService] LoginPacket sent → waiting for response...");

            // 4. Đợi kết quả với timeout
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
            Logger.Debug($"[AccountService] AuthenticateAsync completed → {(finalResult.IsSuccess ? "SUCCESS" : "FAILURE")}");
            return finalResult;
        }
        catch (System.OperationCanceledException)
        {
            Logger.Debug("[AccountService] AuthenticateAsync was canceled by user.");
            return LoginResult.Failure("Đăng nhập bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            Logger.Error($"[AccountService] AuthenticateAsync exception: {ex}");
            if (ex.InnerException != null)
            {
                Logger.Error($"InnerException: {ex.InnerException}");
            }

            return LoginResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    // --- Error mapping --------------------------------------------------------

    private static LoginResult MapErrorResponse(ProtocolReason reason, ProtocolAdvice advice)
    {
        System.String message = reason switch
        {
            ProtocolReason.NOT_FOUND => "Tài khoản không tồn tại.",
            ProtocolReason.MALFORMED_PACKET => "Gói tin không hợp lệ.",
            ProtocolReason.INTERNAL_ERROR => "Lỗi hệ thống, vui lòng thử lại sau.",
            ProtocolReason.UNAUTHENTICATED => "Sai mật khẩu, vui lòng kiểm tra lại.",
            ProtocolReason.FORBIDDEN => "Tài khoản bị cấm hoặc chưa được kích hoạt, vui lòng liên hệ quản trị viên.",
            ProtocolReason.ACCOUNT_LOCKED => "Tài khoản tạm bị khóa do nhập sai nhiều lần, vui lòng thử lại sau 15 phút.",
            _ => "Đăng nhập thất bại, vui lòng thử lại."
        };

        return LoginResult.Failure(message, advice);
    }
}
