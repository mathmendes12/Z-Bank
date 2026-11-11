using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZBank.Application.Common.Interfaces;
using ZBank.Application.Ledger;
using ZBank.Domain.Accounts;
using ZBank.Domain.Users;
using ZBank.Infrastructure.Data;

namespace ZBank.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _logger.LogInformation("Creating database schema...");
            await dbContext.Database.EnsureCreatedAsync(stoppingToken);

            _logger.LogInformation("Checking for seed data...");

            var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Cpf == "00000000000", stoppingToken);
            if (adminUser == null)
            {
                _logger.LogInformation("Seeding admin user and accounts...");

                adminUser = User.Create("00000000000"); // Admin User
                dbContext.Users.Add(adminUser);

                var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
                var adminCredential = new Credential(adminUser.Id, passwordHasher.Hash("123456"));
                dbContext.Credentials.Add(adminCredential);

                var adminAccount = new Account(Guid.NewGuid(), adminUser.Id, DateTimeOffset.UtcNow);
                dbContext.Accounts.Add(adminAccount);

                var systemUser = User.Create("99999999999"); // System User
                dbContext.Users.Add(systemUser);

                var systemAccount = new Account(Guid.NewGuid(), systemUser.Id, DateTimeOffset.UtcNow);
                dbContext.Accounts.Add(systemAccount);

                await dbContext.SaveChangesAsync(stoppingToken);

                var groupId = Guid.NewGuid();
                var now = DateTimeOffset.UtcNow;

                _logger.LogInformation("Seeding initial ledger entries...");
                var systemEntry = new LedgerEntry(Guid.NewGuid(), systemAccount.Id, -1000000, OperationType.Transfer, groupId, now);
                var adminEntry = new LedgerEntry(Guid.NewGuid(), adminAccount.Id, 1000000, OperationType.Transfer, groupId, now);
                
                systemAccount.UpdateBalance(-1000000, now);
                adminAccount.UpdateBalance(1000000, now);

                dbContext.LedgerEntries.Add(systemEntry);
                dbContext.LedgerEntries.Add(adminEntry);

                await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Seed completed successfully.");
            }
            else
            {
                _logger.LogInformation("Seed data already exists.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(60000, stoppingToken);
        }
    }
}
