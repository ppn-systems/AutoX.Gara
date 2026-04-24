// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;using AutoX.Gara.Frontend.Models.Results.Accounts;using AutoX.Gara.Shared.Enums;using AutoX.Gara.Shared.Protocol.Auth;using Microsoft.Extensions.Logging;using Nalix.Common.Networking.Protocols;using Nalix.Common.Primitives;
using Nalix.Framework.Configuration;using Nalix.Framework.DataFrames.SignalFrames;using Nalix.Framework.Injection;using Nalix.SDK.Options;using Nalix.SDK.Transport;using Nalix.SDK.Transport.Extensions;using System;using System.Threading;using System.Threading.Tasks;

namespace AutoX.Gara.Frontend.Services.Accounts;

/// <summary>
/// Dịch vụ xử lý nghiệp vụ tài khoản và kết nối hệ thống.
/// Tuân thủ tiêu chuẩn code công nghiệp: Clean Code, DI-ready, Logging, và Error Handling.
/// </summary>
public sealed class AccountService : IAccountService
{
    private const int LoginTimeoutMs = 5_000;

    /// <summary>
    /// Logger được lấy từ InstanceManager để ghi lại vết sự cố (Traceability).
    /// </summary>
    private ILogger Logger => InstanceManager.Instance.GetOrCreateInstance<ILogger>();

    /// <summary>
    /// Thực hiện kết nối TCP và Handshake bảo mật (X25519).
    /// </summary>
    /// <param name="ct">Token hủy bỏ tác vụ.</param>
    /// <returns>Kết quả trạng thái kết nối.</returns>
    public async Task<ConnectionResult> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            var client = InstanceManager.Instance.GetExistingInstance<TcpSession>();
            if (client == null)
            {
                return ConnectionResult.Failure("TcpSession chưa được đăng ký trong hệ thống.");
            }

            var options = ConfigurationManager.Instance.Get<TransportOptions>();
            if (options == null)
            {
                return ConnectionResult.Failure("Cấu hình mạng (TransportOptions) không tồn tại.");
            }

            // 1. Thiết lập kết nối TCP mức thấp dựa trên cấu hình tập trung
            if (options.Secret.IsZero)
            {
                client.Options.EncryptionEnabled = false;
                client.Options.Secret = Bytes32.Zero;
            }

            await client.ConnectAsync(options.Address, options.Port, ct).ConfigureAwait(false);

            // 2. Thực hiện Handshake mã hóa X25519 để thiết lập Session Key bảo mật
            await client.HandshakeAsync(ct).ConfigureAwait(false);

            Logger.LogInformation("[AccountService] Đã thiết lập kết nối bảo mật tới {Host}:{Port}", options.Address, options.Port);
            return ConnectionResult.Success();
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("[AccountService] Kết nối bị hủy bởi người dùng.");
            return ConnectionResult.Failure("Kết nối bị hủy.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[AccountService] Lỗi kết nối server: {Message}", ex.Message);
            return ConnectionResult.Failure($"Không thể kết nối tới máy chủ: {ex.Message}");
        }
    }

    /// <summary>
    /// Xác thực người dùng thông qua giao thức Nalix RPC.
    /// </summary>
    /// <param name="username">Tên đăng nhập.</param>
    /// <param name="password">Mật khẩu (Clear-text phục vụ xác thực một lần).</param>
    /// <param name="ct">Token hủy bỏ tác vụ.</param>
    /// <returns>Kết quả đăng nhập.</returns>
    public async Task<LoginResult> AuthenticateAsync(string username, string password, CancellationToken ct = default)
    {
        try
        {
            var client = InstanceManager.Instance.GetExistingInstance<TcpSession>();
            if (client == null || !client.IsConnected)
            {
                return LoginResult.Failure("Chưa có kết nối tới máy chủ.", ProtocolAdvice.DO_NOT_RETRY);
            }

            // Khởi tạo Packet Login với data model chuẩn hóa
            var packet = new LoginPacket();
            var model = new LoginRequestModel { Username = username, Password = password };
            packet.Initialize((ushort)OpCommand.LOGIN, model);

            // Gửi yêu cầu RPC với tùy chọn Timeout và Encrypt (Bắt buộc sau Handshake)
            var response = await client.RequestAsync<Directive>(
                packet,
                options: RequestOptions.Default.WithTimeout(LoginTimeoutMs).WithEncrypt(),
                ct: ct).ConfigureAwait(false);

            // Xử lý phản hồi từ Server (Directive frame)
            if (response.Type == ControlType.NONE)
            {
                Logger.LogInformation("[AccountService] Người dùng '{User}' đăng nhập thành công.", username);
                return LoginResult.Success();
            }

            return MapErrorResponse(response.Reason, response.Action);
        }
        catch (OperationCanceledException)
        {
            return LoginResult.Failure("Đăng nhập bị hủy.", ProtocolAdvice.NONE);
        }
        catch (TimeoutException)
        {
            Logger.LogWarning("[AccountService] Hết hạn chờ phản hồi từ Server (Login).");
            return LoginResult.Timeout();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[AccountService] Lỗi xác thực tài khoản: {Message}", ex.Message);
            return LoginResult.Failure($"Lỗi hệ thống: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    /// <summary>
    /// Ánh xạ mã lỗi từ giao thức sang thông điệp người dùng thân thiện.
    /// </summary>
    private static LoginResult MapErrorResponse(ProtocolReason reason, ProtocolAdvice advice)
    {
        string message = reason switch
        {
            ProtocolReason.NOT_FOUND => "Tài khoản không tồn tại trong hệ thống.",
            ProtocolReason.UNAUTHENTICATED => "Mật khẩu không chính xác.",
            ProtocolReason.STATE_VIOLATION => "Phiên làm việc không hợp lệ (Handshake Error).",
            _ => "Máy chủ từ chối đăng nhập (Lỗi giao thức)."
        };

        return LoginResult.Failure(message, advice);
    }
}

