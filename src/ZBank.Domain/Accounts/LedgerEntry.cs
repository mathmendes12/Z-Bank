using System;

namespace ZBank.Domain.Accounts;

public class LedgerEntry
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public long Amount { get; init; } // In cents
    public OperationType OperationType { get; init; }
    public Guid TransactionGroupId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public LedgerEntry(Guid id, Guid accountId, long amount, OperationType operationType, Guid transactionGroupId, DateTimeOffset createdAt)
    {
        Id = id;
        AccountId = accountId;
        Amount = amount;
        OperationType = operationType;
        TransactionGroupId = transactionGroupId;
        CreatedAt = createdAt;
    }
}
