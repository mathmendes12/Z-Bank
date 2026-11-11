using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZBank.Application.Ledger;
using ZBank.Domain.Accounts;
using ZBank.Infrastructure.Data;
using ZBank.Application.Common.Interfaces;

namespace ZBank.IntegrationTests.Ledger;

public class AccountBalanceConcurrencyTests : IntegrationTestBase
{
    [Fact]
    public async Task ConcurrentTransfers_ShouldMaintainConsistentBalance_AndZeroSum()
    {
        // Arrange
        var account1Id = Guid.NewGuid();
        var account2Id = Guid.NewGuid();

        // Seed initial accounts
        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var account1 = new Account(account1Id, Guid.NewGuid(), DateTimeOffset.UtcNow);
            account1.UpdateBalance(10000, DateTimeOffset.UtcNow); // 100 reais
            var account2 = new Account(account2Id, Guid.NewGuid(), DateTimeOffset.UtcNow);

            await dbContext.Accounts.AddAsync(account1);
            await dbContext.Accounts.AddAsync(account2);
            await dbContext.SaveChangesAsync();
        }

        int concurrencyLevel = 10;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < concurrencyLevel; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var scope = Factory.Services.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ILedgerTransactionService>();

                var requests = new List<LedgerEntryRequest>
                {
                    new LedgerEntryRequest { AccountId = account1Id, Amount = -100, OperationType = OperationType.Transfer },
                    new LedgerEntryRequest { AccountId = account2Id, Amount = 100, OperationType = OperationType.Transfer }
                };

                await service.ExecuteTransactionAsync(Guid.NewGuid(), requests);
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var account1 = await dbContext.Accounts.FindAsync(account1Id);
            var account2 = await dbContext.Accounts.FindAsync(account2Id);

            // Initial 10000, 10 transfers of 100 -> final 9000
            Assert.Equal(9000, account1!.Balance);
            
            // Initial 0, 10 transfers of 100 -> final 1000
            Assert.Equal(1000, account2!.Balance);

            // Zero sum verification
            var totalSum = await dbContext.LedgerEntries.SumAsync(e => e.Amount);
            Assert.Equal(0, totalSum);
        }
    }
}
