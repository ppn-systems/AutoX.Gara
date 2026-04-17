using AutoX.Gara.Shared.Enums;
using Nalix.Common.Networking.Protocols;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Shared.Protocol.Auth;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using AutoX.Gara.Api.Handlers.Common;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.DataFrames.Pooling;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Framework.Serialization;
using System;
using System.Collections.Concurrent;

namespace AutoX.Gara.Api.Handlers.Auth;

/// <summary>
/// Packet Handler for account related operations (Login, Register).
/// </summary>
[PacketController]
public sealed class AccountHandler(IAccountAppService accountService, IDataSessionFactory dataSessionFactory)
{
    private readonly IAccountAppService _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    
    private const string AttributeUsername = "Username";


    [PacketPermission(PermissionLevel.NONE)]
    [PacketOpcode((ushort)OpCommand.LOGIN)]
    [PacketRateLimit(requestsPerSecond: 1, burst: 1)]
    public async ValueTask LoginAsync(IPacketContext<LoginPacket> context)
    {
        LoginPacket packet = context.Packet;
        IConnection connection = context.Connection;

        // Gọi Business Logic Service
        var result = await _accountService.AuthenticateAsync(packet.Account.Username, packet.Account.Password).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        // Đăng ký session trong hạ tầng Networking
        connection.Level = Enum.Parse<PermissionLevel>(result.Data!.Role);
        connection.OnCloseEvent += OnAccountLogout;
        connection.Attributes[AttributeUsername] = result.Data.Username;

        await context.OkAsync().ConfigureAwait(false);
    }

    [PacketPermission(PermissionLevel.NONE)]
    [PacketRateLimit(requestsPerSecond: 1, burst: 1)]
    [PacketOpcode((ushort)OpCommand.REGISTER)]
    public async ValueTask RegisterAsync(IPacketContext<LoginPacket> context)
    {
        LoginPacket packet = context.Packet;
        IConnection connection = context.Connection;

        var result = await _accountService.RegisterAsync(packet.Account.Username, packet.Account.Password).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await context.OkAsync().ConfigureAwait(false);
    }

    private async void OnAccountLogout(object sender, IConnectEventArgs args)
    {
        args.Connection.OnCloseEvent -= OnAccountLogout;

        if (!args.Connection.Attributes.TryGetValue(AttributeUsername, out object val) || val is not string username || string.IsNullOrEmpty(username))
        {
            return;
        }

        args.Connection.Attributes.Remove(AttributeUsername);

        try
        {
            await using var session = _dataSessionFactory.Create();
            var account = await session.Accounts.GetByUsernameAsync(username).ConfigureAwait(false);
            if (account is not null)
            {
                account.Deactivate();
                await session.Accounts.SaveChangesAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?.Error($"[AccountHandler] Logout failed for {username}: {ex.Message}");
        }
    }
}
