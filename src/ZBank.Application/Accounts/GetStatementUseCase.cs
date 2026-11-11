using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Accounts;

namespace ZBank.Application.Accounts;

public class AccountStatementResponse
{
    public IEnumerable<LedgerEntry> Entries { get; set; } = new List<LedgerEntry>();
}

public interface IGetStatementUseCase
{
    Task<AccountStatementResponse?> ExecuteAsync(Guid userId, int page = 1, int limit = 50);
}

public class GetStatementUseCase : IGetStatementUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILedgerEntryRepository _ledgerEntryRepository;

    public GetStatementUseCase(IAccountRepository accountRepository, ILedgerEntryRepository ledgerEntryRepository)
    {
        _accountRepository = accountRepository;
        _ledgerEntryRepository = ledgerEntryRepository;
    }

    public async Task<AccountStatementResponse?> ExecuteAsync(Guid userId, int page = 1, int limit = 50)
    {
        var account = await _accountRepository.GetByUserIdAsync(userId);
        if (account == null)
            return null;

        var entries = await _ledgerEntryRepository.GetByAccountIdAsync(account.Id, page, limit);

        return new AccountStatementResponse { Entries = entries };
    }
}
