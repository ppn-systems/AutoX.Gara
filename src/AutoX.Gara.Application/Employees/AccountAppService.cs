using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Domain.Entities.Identity;
using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Models;
using Nalix.Common.Security;
using Nalix.Framework.Security.Hashing;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Employees;

public sealed class AccountAppService(IDataSessionFactory dataSessionFactory, ILogger<AccountAppService> logger) : IAccountAppService
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<AccountAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ServiceResult<AuthData>> AuthenticateAsync(string username, string password)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var account = await session.Accounts.GetByUsernameAsync(username).ConfigureAwait(false);

            if (account == null)
            {
                return ServiceResult<AuthData>.Failure("Tài khoản không tồn tại.", ProtocolReason.NOT_FOUND);
            }

            if (!account.IsActive)
            {
                return ServiceResult<AuthData>.Failure("Tài khoản đã bị khóa.", ProtocolReason.RATE_LIMITED);
            }

            if (!Pbkdf2.Verify(password, account.Salt, account.Hash))
            {
                return ServiceResult<AuthData>.Failure("Mật khẩu không chính xác.", ProtocolReason.UNAUTHORIZED);
            }

            account.Activate();
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
            var normalizedUsername = username.Trim().ToLower();

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

internal static class AccountValidation
{
    public static bool IsValidUsername(string username) => !string.IsNullOrWhiteSpace(username) && username.Length >= 3;
    public static bool IsValidPassword(string password) => !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
}