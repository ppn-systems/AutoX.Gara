// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Validation;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Security.Hashing;
using System;
using System.Threading.Tasks;
namespace AutoX.Gara.Application.Employees;
public sealed class AccountAppService(IDataSessionFactory dataSessionFactory, ILogger<AccountAppService> logger)
{
    private const byte MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LoginLockWindow = TimeSpan.FromMinutes(15);
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<AccountAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    public async Task<ServiceResult<AuthData>> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return ServiceResult<AuthData>.Failure("Thông tin đăng nhập không hợp lệ.", ProtocolReason.MALFORMED_PACKET);
        }
        string normalizedUsername = username.Trim().ToLowerInvariant();
        try
        {
            await using var session = _dataSessionFactory.Create();
            var account = await session.Accounts.GetByUsernameAsync(normalizedUsername).ConfigureAwait(false);
            if (account == null)
            {
                return ServiceResult<AuthData>.Failure("Tài khoản không tồn tại.", ProtocolReason.NOT_FOUND);
            }
            if (!account.IsActive)
            {
                return ServiceResult<AuthData>.Failure("Tài khoản đã bị khóa.", ProtocolReason.RATE_LIMITED);
            }
            var now = DateTime.UtcNow;
            bool inLockWindow = account.LastFailedLogin.HasValue
                && now - account.LastFailedLogin.Value <= LoginLockWindow;
            if (inLockWindow && account.FailedLoginAttempts >= MaxFailedLoginAttempts)
            {
                return ServiceResult<AuthData>.Failure("Tài khoản tạm thời bị khóa do nhập sai nhiều lần.", ProtocolReason.RATE_LIMITED);
            }
            if (!inLockWindow && account.FailedLoginAttempts > 0)
            {
                account.FailedLoginAttempts = 0;
                account.LastFailedLogin = null;
            }
            if (!Pbkdf2.Verify(password, account.Salt, account.Hash))
            {
                account.FailedLoginAttempts = (byte)Math.Min(byte.MaxValue, account.FailedLoginAttempts + 1);
                account.LastFailedLogin = now;
                await session.Accounts.SaveChangesAsync().ConfigureAwait(false);
                if (account.FailedLoginAttempts >= MaxFailedLoginAttempts)
                {
                    return ServiceResult<AuthData>.Failure("Tài khoản tạm thời bị khóa do nhập sai nhiều lần.", ProtocolReason.RATE_LIMITED);
                }
                return ServiceResult<AuthData>.Failure("Mật khẩu không chính xác.", ProtocolReason.UNAUTHORIZED);
            }
            account.Activate();
            account.FailedLoginAttempts = 0;
            account.LastFailedLogin = null;
            account.LastLogin = now;
            await session.Accounts.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<AuthData>.Success(new AuthData(account.Username, account.Role.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for user {Username}", username);
            return ServiceResult<AuthData>.Failure("Lỗi hệ thống trong quá trình đăng nhập.", ProtocolReason.INTERNAL_ERROR);
        }
    }
    public async Task<ServiceResult<AuthData>> RegisterAsync(string username, string password)
    {
        if (!AccountValidation.IsValidUsername(username))
        {
            return ServiceResult<AuthData>.Failure("Tên đăng nhập không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }
        if (!AccountValidation.IsValidPassword(password))
        {
            return ServiceResult<AuthData>.Failure("Mật khẩu không đủ mạnh.", ProtocolReason.VALIDATION_FAILED);
        }
        try
        {
            await using var session = _dataSessionFactory.Create();
            var normalizedUsername = username.Trim().ToLowerInvariant();
            if (await session.Accounts.ExistsByUsernameAsync(normalizedUsername).ConfigureAwait(false))
            {
                return ServiceResult<AuthData>.Failure("Tài khoản đã tồn tại.", ProtocolReason.ALREADY_EXISTS);
            }
            Pbkdf2.Hash(password, out byte[] salt, out byte[] hash);
            var newAccount = new Account
            {
                Username = normalizedUsername,
                Salt = salt,
                Hash = hash,
                Role = PermissionLevel.USER
            };
            newAccount.Activate();
            await session.Accounts.AddAsync(newAccount).ConfigureAwait(false);
            await session.Accounts.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<AuthData>.Success(new AuthData(newAccount.Username, newAccount.Role.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", username);
            return ServiceResult<AuthData>.Failure("Lỗi hệ thống trong quá trình đăng ký.", ProtocolReason.INTERNAL_ERROR);
        }
    }
}
