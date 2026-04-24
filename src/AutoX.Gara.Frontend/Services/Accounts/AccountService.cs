// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Models.Results.Accounts;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Auth;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Primitives;
using Nalix.Framework.Configuration;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.Injection;
using Nalix.SDK.Options;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend.Services.Accounts;

public sealed class AccountService : IAccountService
{
    private const int RequestTimeoutMs = 5_000;

    private ILogger Logger => InstanceManager.Instance.GetOrCreateInstance<ILogger>();

    public async Task<ConnectionResult> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            var client = InstanceManager.Instance.GetExistingInstance<TcpSession>();
            if (client is null)
            {
                return ConnectionResult.Failure("TcpSession chưa được đăng ký trong hệ thống.");
            }

            var options = ConfigurationManager.Instance.Get<TransportOptions>();
            if (options is null)
            {
                return ConnectionResult.Failure("Cấu hình mạng (TransportOptions) không tồn tại.");
            }

            if (options.Secret.IsZero)
            {
                client.Options.EncryptionEnabled = false;
                client.Options.Secret = Bytes32.Zero;
            }

            await client.ConnectAsync(options.Address, options.Port, ct).ConfigureAwait(false);
            await client.HandshakeAsync(ct).ConfigureAwait(false);

            Logger.LogInformation("[AccountService] Connected to {Host}:{Port}.", options.Address, options.Port);
            return ConnectionResult.Success();
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("[AccountService] Connection canceled.");
            return ConnectionResult.Failure("Kết nối bị hủy.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[AccountService] Connect failed: {Message}", ex.Message);
            return ConnectionResult.Failure($"Không thể kết nối tới máy chủ: {ex.Message}");
        }
    }

    public Task<LoginResult> AuthenticateAsync(string username, string password, CancellationToken ct = default)
        => SendAccountCommandAsync((ushort)OpCommand.LOGIN, username, password, "login", ct);

    public Task<LoginResult> RegisterAsync(string username, string password, CancellationToken ct = default)
        => SendAccountCommandAsync((ushort)OpCommand.REGISTER, username, password, "register", ct);

    private async Task<LoginResult> SendAccountCommandAsync(
        ushort opCode,
        string username,
        string password,
        string operationName,
        CancellationToken ct)
    {
        try
        {
            var client = InstanceManager.Instance.GetExistingInstance<TcpSession>();
            if (client is null || !client.IsConnected)
            {
                return LoginResult.Failure("Chưa có kết nối tới máy chủ.", ProtocolAdvice.DO_NOT_RETRY);
            }

            var packet = new LoginPacket();
            var model = new LoginRequestModel
            {
                Username = username,
                Password = password,
            };
            packet.Initialize(opCode, model);

            var response = await client.RequestAsync<Directive>(
                packet,
                options: RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(),
                ct: ct).ConfigureAwait(false);

            if (response.Type == ControlType.NONE)
            {
                Logger.LogInformation("[AccountService] {Operation} success for user '{User}'.", operationName, username);
                return LoginResult.Success();
            }

            return MapErrorResponse(response.Reason, response.Action, operationName);
        }
        catch (OperationCanceledException)
        {
            return LoginResult.Failure("Thao tác bị hủy.", ProtocolAdvice.NONE);
        }
        catch (TimeoutException)
        {
            Logger.LogWarning("[AccountService] Timeout during {Operation}.", operationName);
            return LoginResult.Timeout();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[AccountService] {Operation} failed: {Message}", operationName, ex.Message);
            return LoginResult.Failure($"Lỗi hệ thống: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static LoginResult MapErrorResponse(ProtocolReason reason, ProtocolAdvice advice, string operationName)
    {
        string message = reason switch
        {
            ProtocolReason.NOT_FOUND when operationName == "login" => "Tài khoản không tồn tại trong hệ thống.",
            ProtocolReason.UNAUTHENTICATED when operationName == "login" => "Mật khẩu không chính xác.",
            ProtocolReason.STATE_VIOLATION => "Phiên làm việc không hợp lệ (Handshake Error).",
            _ when operationName == "register" => "Đăng ký thất bại. Vui lòng thử lại.",
            _ => "Máy chủ từ chối đăng nhập.",
        };

        return LoginResult.Failure(message, advice);
    }
}
