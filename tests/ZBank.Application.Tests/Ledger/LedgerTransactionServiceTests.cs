using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ZBank.Application.Common.Interfaces;
using ZBank.Application.Ledger;
using ZBank.Domain.Accounts;
using ZBank.Domain.Exceptions;

namespace ZBank.Application.Tests.Ledger;

public class LedgerTransactionServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock;
    private readonly Mock<ILedgerEntryRepository> _ledgerRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly LedgerTransactionService _service;

    public LedgerTransactionServiceTests()
    {
        _accountRepoMock = new Mock<IAccountRepository>();
        _ledgerRepoMock = new Mock<ILedgerEntryRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _service = new LedgerTransactionService(_accountRepoMock.Object, _ledgerRepoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_ShouldFail_WhenSumIsNotZero()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var requests = new List<LedgerEntryRequest>
        {
            new LedgerEntryRequest { AccountId = Guid.NewGuid(), Amount = 1000, OperationType = OperationType.Deposit },
            new LedgerEntryRequest { AccountId = Guid.NewGuid(), Amount = -500, OperationType = OperationType.Withdrawal }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ExecuteTransactionAsync(groupId, requests));
        Assert.Equal("A soma das operações da transação deve ser exatamente zero.", ex.Message);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_ShouldFail_WhenInsufficientFunds()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var account1Id = Guid.NewGuid();
        var account2Id = Guid.NewGuid();

        var requests = new List<LedgerEntryRequest>
        {
            new LedgerEntryRequest { AccountId = account1Id, Amount = -1000, OperationType = OperationType.Transfer },
            new LedgerEntryRequest { AccountId = account2Id, Amount = 1000, OperationType = OperationType.Transfer }
        };

        var account1 = new Account(account1Id, Guid.NewGuid(), DateTimeOffset.UtcNow); // Balance 0
        var account2 = new Account(account2Id, Guid.NewGuid(), DateTimeOffset.UtcNow);

        _accountRepoMock.Setup(x => x.GetByIdForUpdateAsync(account1Id)).ReturnsAsync(account1);
        _accountRepoMock.Setup(x => x.GetByIdForUpdateAsync(account2Id)).ReturnsAsync(account2);

        // Act & Assert
        await Assert.ThrowsAsync<InsufficientFundsException>(() => _service.ExecuteTransactionAsync(groupId, requests));
    }

    [Fact]
    public async Task ExecuteTransactionAsync_ShouldSucceed_WhenValid()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var account1Id = Guid.NewGuid();
        var account2Id = Guid.NewGuid();

        var requests = new List<LedgerEntryRequest>
        {
            new LedgerEntryRequest { AccountId = account1Id, Amount = -500, OperationType = OperationType.Transfer },
            new LedgerEntryRequest { AccountId = account2Id, Amount = 500, OperationType = OperationType.Transfer }
        };

        var account1 = new Account(account1Id, Guid.NewGuid(), DateTimeOffset.UtcNow);
        account1.UpdateBalance(1000, DateTimeOffset.UtcNow); // Has 1000
        
        var account2 = new Account(account2Id, Guid.NewGuid(), DateTimeOffset.UtcNow); // Has 0

        _accountRepoMock.Setup(x => x.GetByIdForUpdateAsync(account1Id)).ReturnsAsync(account1);
        _accountRepoMock.Setup(x => x.GetByIdForUpdateAsync(account2Id)).ReturnsAsync(account2);

        // Act
        await _service.ExecuteTransactionAsync(groupId, requests);

        // Assert
        Assert.Equal(500, account1.Balance);
        Assert.Equal(500, account2.Balance);

        _ledgerRepoMock.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<LedgerEntry>>(entries => true)), Times.Once);
        _accountRepoMock.Verify(x => x.UpdateAsync(account1), Times.Once);
        _accountRepoMock.Verify(x => x.UpdateAsync(account2), Times.Once);
        _uowMock.Verify(x => x.CommitAsync(), Times.Once);
    }
}
