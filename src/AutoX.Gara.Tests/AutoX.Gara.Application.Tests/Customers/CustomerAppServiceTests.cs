using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Application.Customers;
using AutoX.Gara.Domain.Entities.Customers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace AutoX.Gara.Application.Tests.Customers;

public class CustomerAppServiceTests
{
    private readonly Mock<IDataSessionFactory> _sessionFactoryMock;
    private readonly Mock<ILogger<CustomerAppService>> _loggerMock;
    private readonly CustomerAppService _service;

    public CustomerAppServiceTests()
    {
        _sessionFactoryMock = new Mock<IDataSessionFactory>();
        _loggerMock = new Mock<ILogger<CustomerAppService>>();
        _service = new CustomerAppService(_sessionFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenEmailExists()
    {
        // Arrange
        var customer = new Customer { Email = "test@example.com", PhoneNumber = "123" };
        var sessionMock = new Mock<IDataSession>();
        var customerRepoMock = new Mock<ICustomerRepository>();
        
        _sessionFactoryMock.Setup(f => f.Create()).Returns(sessionMock.Object);
        sessionMock.Setup(s => s.Customers).Returns(customerRepoMock.Object);
        customerRepoMock.Setup(r => r.ExistsByContactAsync(customer.Email, customer.PhoneNumber))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateAsync(customer);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Email hoặc số điện thoại đã tồn tại.");
    }
}
