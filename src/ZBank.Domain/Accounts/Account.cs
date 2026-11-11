using System;

namespace ZBank.Domain.Accounts;

public class Account
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public AccountStatus Status { get; private set; }
    public long Balance { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Account(Guid id, Guid userId, DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        Status = AccountStatus.Active;
        Balance = 0;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public void UpdateBalance(long amount, DateTimeOffset updatedAt)
    {
        Balance += amount;
        UpdatedAt = updatedAt;
    }
}
