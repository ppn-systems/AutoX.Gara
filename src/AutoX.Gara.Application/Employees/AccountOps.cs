// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Auth;
using AutoX.Gara.Shared.Validation;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Framework.Security.Hashing;
using Nalix.Runtime.Extensions;

namespace AutoX.Gara.Application.Employees;

[PacketController]
public sealed class AccountOps(IDataSessionFactory dataSessionFactory)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new System.ArgumentNullException(nameof(dataSessionFactory));
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Nalix.Common.Identity.ISnowflake, System.String> ActiveUsers = new();

    [PacketPermission(PermissionLevel.NONE)]
    [PacketOpcode((System.UInt16)OpCommand.LOGIN)]
    [PacketRateLimit(requestsPerSecond: 1, burst: 1)]
    public async System.Threading.Tasks.Task LoginAsync(IPacket p, IConnection connection)
    {
        if (p is not LoginPacket packet)
        {
            await connection.SendAsync(ControlType.ERROR, ProtocolReason.MALFORMED_PACKET, ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, 0, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        await using var session = _dataSessionFactory.Create();
        var accounts = session.Accounts;

        var username = packet.Account.Username.Trim().ToLower();
        var account = await accounts.GetByUsernameAsync(username).ConfigureAwait(false);
        if (account is null)
        {
            await connection.SendAsync(ControlType.ERROR, ProtocolReason.NOT_FOUND, ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        if (account.FailedLoginAttempts >= 5 && account.LastFailedLogin.HasValue && System.DateTime.UtcNow < account.LastFailedLogin.Value.AddMinutes(15))
        {
            await connection.SendAsync(ControlType.ERROR, ProtocolReason.ACCOUNT_LOCKED, ProtocolAdvice.BACKOFF_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        if (!Pbkdf2.Verify(packet.Account.Password, account.Salt, account.Hash))
        {
            account.FailedLoginAttempts++;
            account.LastFailedLogin = System.DateTime.UtcNow;
            await accounts.SaveChangesAsync().ConfigureAwait(false);

            await connection.SendAsync(ControlType.ERROR, ProtocolReason.UNAUTHENTICATED, ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        if (account.IsActive)
        {
            await connection.SendAsync(ControlType.ERROR, ProtocolReason.FORBIDDEN, ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        try
        {
            account.Activate();
            account.FailedLoginAttempts = 0;
            account.LastLogin = System.DateTime.UtcNow;
            await accounts.SaveChangesAsync().ConfigureAwait(false);

            connection.Level = account.Role;
            connection.OnCloseEvent += OnAccountLogout;
            ActiveUsers[connection.ID] = username;

            await connection.SendAsync(ControlType.NONE, ProtocolReason.NONE, ProtocolAdvice.NONE, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?.Error($"[APP.{nameof(AccountOps)}:{nameof(LoginAsync)}] failed-login ep={connection.NetworkEndpoint} ex={ex.Message}");
            await connection.SendAsync(ControlType.ERROR, ProtocolReason.INTERNAL_ERROR, ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
    }

    [PacketPermission(PermissionLevel.NONE)]
    [PacketRateLimit(requestsPerSecond: 1, burst: 1)]
    [PacketOpcode((System.UInt16)OpCommand.REGISTER)]
    public async System.Threading.Tasks.Task RegisterAsync(IPacket p, IConnection connection)
    {
        if (p is not LoginPacket packet)
        {
            await connection.SendAsync(ControlType.ERROR, ProtocolReason.MALFORMED_PACKET, ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, p.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        if (!AccountValidation.IsValidUsername(packet.Account.Username))
        {
            await connection.SendAsync(ControlType.ERROR, ProtocolReason.INVALID_USERNAME, ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        if (!AccountValidation.IsValidPassword(packet.Account.Password))
        {
            await connection.SendAsync(ControlType.ERROR, ProtocolReason.WEAK_PASSWORD, ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        await using var session = _dataSessionFactory.Create();
        var accounts = session.Accounts;
        var username = packet.Account.Username.Trim().ToLower();

        if (await accounts.ExistsByUsernameAsync(username).ConfigureAwait(false))
        {
            await connection.SendAsync(ControlType.ERROR, ProtocolReason.ALREADY_EXISTS, ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        Pbkdf2.Hash(packet.Account.Password, out System.Byte[] salt, out System.Byte[] hash);

        try
        {
            Account newAccount = new() { Username = username, Salt = salt, Hash = hash, Role = PermissionLevel.GUEST };
            newAccount.Deactivate();

            await accounts.AddAsync(newAccount).ConfigureAwait(false);
            await accounts.SaveChangesAsync().ConfigureAwait(false);

            System.Array.Clear(salt, 0, salt.Length);
            System.Array.Clear(hash, 0, hash.Length);
        }
        catch (System.Exception ex)
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?.Error($"[APP.{nameof(AccountOps)}:{nameof(RegisterAsync)}] register-failed username={username} ex={ex.Message}");
            await connection.SendAsync(ControlType.ERROR, ProtocolReason.INTERNAL_ERROR, ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        await connection.SendAsync(ControlType.NONE, ProtocolReason.NONE, ProtocolAdvice.NONE, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
    }

    private async void OnAccountLogout(System.Object sender, IConnectEventArgs args)
    {
        args.Connection.OnCloseEvent -= OnAccountLogout;

        if (!ActiveUsers.TryRemove(args.Connection.ID, out System.String username) || System.String.IsNullOrEmpty(username))
        {
            return;
        }

        await using var session = _dataSessionFactory.Create();
        var account = await session.Accounts.GetByUsernameAsync(username).ConfigureAwait(false);
        if (account is null)
        {
            return;
        }

        account.Deactivate();
        await session.Accounts.SaveChangesAsync().ConfigureAwait(false);
    }
}


