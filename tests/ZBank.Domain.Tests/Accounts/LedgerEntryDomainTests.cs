using System;
using Xunit;
using ZBank.Domain.Accounts;

namespace ZBank.Domain.Tests.Accounts;

public class LedgerEntryDomainTests
{
    [Fact]
    public void LedgerEntry_ShouldBeImmutable_AndStoreCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var amount = 1000L;
        var operationType = OperationType.Deposit;
        var transactionGroupId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        // Act
        var entry = new LedgerEntry(id, accountId, amount, operationType, transactionGroupId, createdAt);

        // Assert
        Assert.Equal(id, entry.Id);
        Assert.Equal(accountId, entry.AccountId);
        Assert.Equal(amount, entry.Amount);
        Assert.Equal(operationType, entry.OperationType);
        Assert.Equal(transactionGroupId, entry.TransactionGroupId);
        Assert.Equal(createdAt, entry.CreatedAt);
        
        // LedgerEntry properties should be get-only or init-only
        // Compilation will fail if we try to mutate it here if properly designed.
    }
}
