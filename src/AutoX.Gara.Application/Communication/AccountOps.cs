// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Auth;
using AutoX.Gara.Shared.Validation;
using Microsoft.EntityFrameworkCore;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Common.Networking.Abstractions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Attributes;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Shared.Security.Credentials;

namespace AutoX.Gara.Application.Communication;

/// <summary>
/// Dịch vụ quản lý tài khoản người dùng, bao gồm đăng ký, đăng nhập, xóa tài khoản và cập nhật mật khẩu.
/// </summary>
/// <remarks>
/// Sử dụng DbContextFactory để tạo context "mới" mỗi lần request (tránh lỗi multi-thread).
/// </remarks>
[PacketController]
public sealed class AccountOps(AutoXDbContextFactory dbContextFactory)
{
    private readonly AutoXDbContextFactory _dbContextFactory = dbContextFactory
        ?? throw new System.ArgumentNullException(nameof(dbContextFactory));

    [PacketPermission(PermissionLevel.NONE)]
    [PacketOpcode((System.UInt16)OpCommand.LOGIN)]
    [PacketRateLimit(requestsPerSecond: 1, burst: 1)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "<Pending>")]
    public async System.Threading.Tasks.Task LoginAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not LoginPacket packet)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps0 ? ps0.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        // Tạo context mới cho mỗi lệnh
        await using var context = _dbContextFactory.CreateDbContext();
        var account = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == packet.Account.Username.Trim().ToLower());

        if (account == null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.NOT_FOUND,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        if (account.FailedLoginAttempts >= 5 && account.LastFailedLogin.HasValue &&
            System.DateTime.UtcNow < account.LastFailedLogin.Value.AddMinutes(15))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.ACCOUNT_LOCKED,
                ProtocolAdvice.BACKOFF_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        if (!Pbkdf2.Verify(packet.Account.Password, account.Salt, account.Hash))
        {
            account.FailedLoginAttempts++;
            account.LastFailedLogin = System.DateTime.UtcNow;

            await context.SaveChangesAsync();

            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.UNAUTHENTICATED,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        // SỬA: Nếu không active thì forbidden
        if (!account.IsActive)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.FORBIDDEN,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        try
        {
            account.Activate();
            account.FailedLoginAttempts = 0;
            account.LastLogin = System.DateTime.UtcNow;

            await context.SaveChangesAsync();

            connection.Level = account.Role;
            connection.OnCloseEvent += OnAccountLogout;
            InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                    .AssociateUsername(connection, packet.Account.Username);

            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, packet.SequenceId).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[APP.{nameof(HandshakeOps)}] failed-login ep={connection.RemoteEndPoint} ex={ex.Message}");

            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    [PacketPermission(PermissionLevel.NONE)]
    [PacketRateLimit(requestsPerSecond: 1, burst: 1)]
    [PacketOpcode((System.UInt16)OpCommand.REGISTER)]
    public async System.Threading.Tasks.Task RegisterAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not LoginPacket packet)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps0 ? ps0.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        if (!AccountValidation.IsValidUsername(packet.Account.Username))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INVALID_USERNAME,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        if (!AccountValidation.IsValidPassword(packet.Account.Password))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.WEAK_PASSWORD,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        await using var context = _dbContextFactory.CreateDbContext();
        System.Boolean existed = await context.Set<Account>().AnyAsync(a => a.Username == packet.Account.Username.Trim().ToLower());
        if (existed)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.ALREADY_EXISTS,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        // Hash password & create salt/hash
        Pbkdf2.Hash(packet.Account.Password, out System.Byte[] salt, out System.Byte[] hash);

        Account newAccount = new()
        {
            Username = packet.Account.Username.Trim().ToLower(),
            Salt = salt,
            Hash = hash,
            Role = PermissionLevel.GUEST,
        };

        try
        {
            newAccount.Deactivate();
            await context.Set<Account>().AddAsync(newAccount);
            await context.SaveChangesAsync();

            System.Array.Clear(salt, 0, salt.Length);
            System.Array.Clear(hash, 0, hash.Length);
        }
        catch (System.Exception ex)
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[APP.{nameof(AccountOps)}:{nameof(RegisterAsync)}] register-failed username={packet.Account.Username} ex={ex.Message}");

            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        await connection.SendAsync(
            ControlType.NONE,
            ProtocolReason.NONE,
            ProtocolAdvice.NONE, packet.SequenceId).ConfigureAwait(false);
    }

    [System.Diagnostics.StackTraceHidden]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private async void OnAccountLogout(System.Object sender, IConnectEventArgs args)
    {
        args.Connection.OnCloseEvent -= OnAccountLogout;

        System.String username = InstanceManager.Instance.GetExistingInstance<ConnectionHub>()?
                                                     .GetUsername(args.Connection.ID);

        if (System.String.IsNullOrEmpty(username))
        {
            return;
        }

        await using var context = _dbContextFactory.CreateDbContext();
        var account = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == username);

        if (account != null)
        {
            account.Deactivate();
            await context.SaveChangesAsync();
        }
    }
}