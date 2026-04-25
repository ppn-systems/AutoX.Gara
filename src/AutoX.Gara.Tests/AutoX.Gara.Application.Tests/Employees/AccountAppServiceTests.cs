using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Application.Repositories;
using AutoX.Gara.Application.Employees;
using AutoX.Gara.Domain.Entities.Identity;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Security.Hashing;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
namespace AutoX.Gara.Application.Tests.Employees;
public class AccountAppServiceTests
{
    private readonly Mock<IDataSessionFactory> _sessionFactoryMock = new();
    private readonly Mock<ILogger<AccountAppService>> _loggerMock = new();
    private readonly Mock<IDataSession> _sessionMock = new();
    private readonly Mock<IAccountRepository> _accountRepoMock = new();
    private readonly AccountAppService _service;
    public AccountAppServiceTests()
    {
        _service = new AccountAppService(_sessionFactoryMock.Object, _loggerMock.Object);
        _sessionFactoryMock.Setup(x => x.Create()).Returns(_sessionMock.Object);
        _sessionMock.Setup(x => x.Accounts).Returns(_accountRepoMock.Object);
        _sessionMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);
    }
    [Fact]
    public async Task AuthenticateAsync_ShouldReturnRateLimited_WhenReachedMaxAttemptsWithinWindow()
    {
        var account = CreateAccount("user", "correct-password");
        account.FailedLoginAttempts = 5;
        account.LastFailedLogin = DateTime.UtcNow.AddMinutes(-2);
        _accountRepoMock.Setup(x => x.GetByUsernameAsync("user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        var result = await _service.AuthenticateAsync("user", "wrong-password");
        result.IsSuccess.Should().BeFalse();
        result.Reason.Should().Be(ProtocolReason.RATE_LIMITED);
        result.ErrorMessage.Should().Be("Tài khoản tạm thời bị khóa do nhập sai nhiều lần.");
    }
    [Fact]
    public async Task AuthenticateAsync_ShouldIncreaseFailedAttempts_WhenPasswordInvalid()
    {
        var account = CreateAccount("employee", "correct-password");
        _accountRepoMock.Setup(x => x.GetByUsernameAsync("employee", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _accountRepoMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var result = await _service.AuthenticateAsync("employee", "wrong-password");
        result.IsSuccess.Should().BeFalse();
        result.Reason.Should().Be(ProtocolReason.UNAUTHORIZED);
        account.FailedLoginAttempts.Should().Be(1);
        account.LastFailedLogin.Should().NotBeNull();
        _accountRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task AuthenticateAsync_ShouldResetFailedAttemptsAndSetLastLogin_WhenPasswordValid()
    {
        var account = CreateAccount("admin", "correct-password");
        account.FailedLoginAttempts = 3;
        account.LastFailedLogin = DateTime.UtcNow.AddMinutes(-1);
        _accountRepoMock.Setup(x => x.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _accountRepoMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var result = await _service.AuthenticateAsync("admin", "correct-password");
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Username.Should().Be("admin");
        account.FailedLoginAttempts.Should().Be(0);
        account.LastFailedLogin.Should().BeNull();
        account.LastLogin.Should().NotBeNull();
        _accountRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    private static Account CreateAccount(string username, string password)
    {
        Pbkdf2.Hash(password, out byte[] salt, out byte[] hash);
        var account = new Account
        {
            Username = username,
            Role = PermissionLevel.USER,
            Salt = salt,
            Hash = hash
        };
        account.Activate();
        return account;
    }
}

