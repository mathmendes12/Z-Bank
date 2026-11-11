using System;
using Xunit;
using ZBank.Domain.Accounts;

namespace ZBank.Domain.Tests.Accounts;

public class AccountDomainTests
{
    [Fact]
    public void Account_ShouldInitializeCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        // Act
        var account = new Account(id, userId, createdAt);

        // Assert
        Assert.Equal(id, account.Id);
        Assert.Equal(userId, account.UserId);
        Assert.Equal(0L, account.Balance);
        Assert.Equal(AccountStatus.Active, account.Status);
        Assert.Equal(createdAt, account.CreatedAt);
        Assert.Equal(createdAt, account.UpdatedAt);
    }
}
