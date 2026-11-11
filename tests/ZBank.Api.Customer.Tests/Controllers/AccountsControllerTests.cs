using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ZBank.Api.Customer.Controllers;
using ZBank.Application.Accounts;
using ZBank.Domain.Accounts;

namespace ZBank.Api.Customer.Tests.Controllers;

public class AccountsControllerTests
{
    private readonly Mock<IGetBalanceUseCase> _getBalanceUseCaseMock;
    private readonly Mock<IGetStatementUseCase> _getStatementUseCaseMock;
    private readonly AccountsController _controller;
    private readonly Guid _userId;

    public AccountsControllerTests()
    {
        _getBalanceUseCaseMock = new Mock<IGetBalanceUseCase>();
        _getStatementUseCaseMock = new Mock<IGetStatementUseCase>();
        
        _userId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
        }, "mock"));

        _controller = new AccountsController();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetBalance_ShouldReturnOk_WithBalance()
    {
        // Arrange
        var expectedResponse = new AccountBalanceResponse { Balance = 10000 };
        _getBalanceUseCaseMock.Setup(x => x.ExecuteAsync(_userId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetBalance(_getBalanceUseCaseMock.Object) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(expectedResponse, result.Value);
    }

    [Fact]
    public async Task GetBalance_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        _getBalanceUseCaseMock.Setup(x => x.ExecuteAsync(_userId))
            .ReturnsAsync((AccountBalanceResponse?)null);

        // Act
        var result = await _controller.GetBalance(_getBalanceUseCaseMock.Object) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task GetStatement_ShouldReturnOk_WithEntries()
    {
        // Arrange
        var entries = new List<LedgerEntry>
        {
            new LedgerEntry(Guid.NewGuid(), Guid.NewGuid(), 5000, OperationType.Deposit, Guid.NewGuid(), DateTimeOffset.UtcNow)
        };
        var expectedResponse = new AccountStatementResponse { Entries = entries };

        _getStatementUseCaseMock.Setup(x => x.ExecuteAsync(_userId, 1, 50))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetStatement(_getStatementUseCaseMock.Object, 1, 50) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(expectedResponse, result.Value);
    }
}
