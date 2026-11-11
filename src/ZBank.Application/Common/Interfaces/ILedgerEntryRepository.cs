using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZBank.Domain.Accounts;

namespace ZBank.Application.Common.Interfaces;

public interface ILedgerEntryRepository
{
    Task AddAsync(LedgerEntry entry);
    Task AddRangeAsync(IEnumerable<LedgerEntry> entries);
    Task<IEnumerable<LedgerEntry>> GetByAccountIdAsync(Guid accountId, int page, int limit);
}
