// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Packets;
using AutoX.Gara.Shared.Validator;
using Nalix.Common.Connection;
using Nalix.Common.Diagnostics;
using Nalix.Common.Enums;
using Nalix.Common.Infrastructure.Connection;
using Nalix.Common.Messaging.Packets.Abstractions;
using Nalix.Common.Messaging.Packets.Attributes;
using Nalix.Common.Messaging.Protocols;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Shared.Security.Credentials;

namespace AutoX.Gara.Application.Communication;

/// <summary>
/// Dịch vụ quản lý tài khoản người dùng, bao gồm đăng ký, đăng nhập, xóa tài khoản và cập nhật mật khẩu.
/// </summary>
/// <remarks>
/// Khởi tạo AccountService với DbContext.
/// </remarks>
/// <param name="context">Context của cơ sở dữ liệu để thao tác với bảng Accounts.</param>
[PacketController]
public sealed class AccountOps(AutoXDbContext context)
{
    private readonly DataRepository<Account> s_account = new(context);

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.NONE)]
    [PacketOpcode((System.UInt16)OpCommand.LOGIN)]
    [PacketRateLimit(requestsPerSecond: 1, burst: 1)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "<Pending>")]
    public async System.Threading.Tasks.Task LoginAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not AccountPacket packet)
        {
            if (p is not IPacketSequenced ps)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.MALFORMED_PACKET,
                    ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);

                return;
            }

            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, ps.SequenceId).ConfigureAwait(false);

            return;
        }

        Account account = await s_account.GetFirstOrDefaultAsync(a => a.Username == packet.Account.Username.Trim().ToLower());

        if (account == null)
        {
            var allAccounts = await s_account.GetAsync(pageSize: 10); // lấy 1000 user đầu; tuỳ database
            System.Console.WriteLine("==== Debug: Username trong database ====");

            foreach (var acc in allAccounts)
            {
                System.Console.WriteLine($"- [{acc.Username}]");
            }


            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.NOT_FOUND,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        if (account.FailedLoginAttempts >= 5 && account.LastFailedLogin.HasValue &&
            System.DateTime.UtcNow < account.LastFailedLogin.Value.AddMinutes(15))
        {
            // ACCOUNT_LOCKED --> Báo cho client là tài khoản đang bị tạm khóa
            // BACKOFF_RETRY  --> Gợi ý: nên đợi thời gian mới thử lại
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

            await s_account.SaveChangesAsync();

            // UNAUTHENTICATED -> Sai mật khẩu
            // FIX_AND_RETRY -> Có thể nhập lại và thử tiếp
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.UNAUTHENTICATED,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        if (!account.IsActive)
        {
            // FORBIDDEN: Tài khoản đúng nhưng bị khóa/chưa được kích hoạt sử dụng
            // 202: Tài khoản bị cấm sử dụng (chưa active)
            // Gợi ý nên liên hệ quản trị viên hoặc CSKH, không tự thử lại được
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

            await s_account.SaveChangesAsync();

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

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.NONE)]
    [PacketOpcode((System.UInt16)OpCommand.REGISTER)]
    public async System.Threading.Tasks.Task RegisterAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not AccountPacket packet)
        {
            if (p is not IPacketSequenced ps)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.MALFORMED_PACKET,
                    ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);

                return;
            }

            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, ps.SequenceId).ConfigureAwait(false);

            return;
        }

        if (!CredentialValidator.IsValidUsername(packet.Account.Username))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INVALID_USERNAME,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        if (!CredentialValidator.IsValidPassword(packet.Account.Password))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.WEAK_PASSWORD,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        if (await s_account.AnyAsync(a => a.Username == packet.Account.Username))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.ALREADY_EXISTS,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        // Hash password & sinh salt
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
            await s_account.AddAsync(newAccount);
            await s_account.SaveChangesAsync();

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

        // TODO (pro): Gửi email xác thực nếu bạn dùng kịch bản kích hoạt email
    }

    [System.Diagnostics.StackTraceHidden]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private async void OnAccountLogout(System.Object sender, IConnectEventArgs args)
    {
        args.Connection.OnCloseEvent -= OnAccountLogout;

        System.String username = InstanceManager.Instance.GetExistingInstance<ConnectionHub>()
                                                         .GetUsername(args.Connection.ID);

        Account account = await s_account.GetFirstOrDefaultAsync(a => a.Username == username);

        if (account != null)
        {
            account.Deactivate();
            await s_account.SaveChangesAsync();
        }
    }
}
