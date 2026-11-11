using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Accounts;
using ZBank.Domain.Exceptions;

namespace ZBank.Application.Ledger;

public class LedgerEntryRequest
{
    public Guid AccountId { get; set; }
    public long Amount { get; set; }
    public OperationType OperationType { get; set; }
}

public interface ILedgerTransactionService
{
    Task ExecuteTransactionAsync(Guid transactionGroupId, List<LedgerEntryRequest> requests);
}

public class LedgerTransactionService : ILedgerTransactionService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILedgerEntryRepository _ledgerEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LedgerTransactionService(
        IAccountRepository accountRepository,
        ILedgerEntryRepository ledgerEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _ledgerEntryRepository = ledgerEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteTransactionAsync(Guid transactionGroupId, List<LedgerEntryRequest> requests)
    {
        if (requests == null || !requests.Any())
            throw new ArgumentException("A transação deve conter pelo menos uma operação.");

        var totalSum = requests.Sum(r => r.Amount);
        if (totalSum != 0)
        {
            throw new InvalidOperationException("A soma das operações da transação deve ser exatamente zero.");
        }

        // ReadCommitted (e nao o Serializable padrao do TransactionScope): a exclusao mutua vem do
        // SELECT ... FOR UPDATE; em Serializable o Postgres aborta o concorrente com 40001 em vez de esperar o lock.
        using var transaction = new System.Transactions.TransactionScope(
            System.Transactions.TransactionScopeOption.Required,
            new System.Transactions.TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted },
            System.Transactions.TransactionScopeAsyncFlowOption.Enabled);

        var entries = new List<LedgerEntry>();
        var now = DateTimeOffset.UtcNow;

        // Sort requests by AccountId to avoid deadlocks
        foreach (var request in requests.OrderBy(r => r.AccountId))
        {
            var account = await _accountRepository.GetByIdForUpdateAsync(request.AccountId);
            if (account == null)
            {
                throw new InvalidOperationException($"Conta {request.AccountId} não encontrada.");
            }

            if (account.Status != AccountStatus.Active)
            {
                throw new InvalidOperationException($"Conta {request.AccountId} não está ativa.");
            }

            if (request.Amount < 0 && account.Balance + request.Amount < 0)
            {
                throw new InsufficientFundsException();
            }

            account.UpdateBalance(request.Amount, now);
            await _accountRepository.UpdateAsync(account);

            var entry = new LedgerEntry(
                Guid.NewGuid(),
                account.Id,
                request.Amount,
                request.OperationType,
                transactionGroupId,
                now
            );
            
            entries.Add(entry);
        }

        await _ledgerEntryRepository.AddRangeAsync(entries);
        await _unitOfWork.CommitAsync();

        transaction.Complete();
    }
}
