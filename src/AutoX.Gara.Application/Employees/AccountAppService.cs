// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;using AutoX.Gara.Domain.Entities.Identity;using AutoX.Gara.Shared.Models;using AutoX.Gara.Shared.Validation;using Microsoft.Extensions.Logging;using Nalix.Common.Networking.Protocols;using Nalix.Common.Security;using Nalix.Framework.Security.Hashing;using System;using System.Threading.Tasks;

namespace AutoX.Gara.Application.Employees;

public sealed class AccountAppService(IDataSessionFactory dataSessionFactory, ILogger<AccountAppService> logger)
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


