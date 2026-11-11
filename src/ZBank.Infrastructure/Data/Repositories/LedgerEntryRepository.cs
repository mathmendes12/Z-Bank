using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Accounts;
using ZBank.Infrastructure.Data;

namespace ZBank.Infrastructure.Data.Repositories;

public class LedgerEntryRepository : ILedgerEntryRepository
{
    private readonly AppDbContext _context;

    public LedgerEntryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LedgerEntry entry)
    {
        await _context.LedgerEntries.AddAsync(entry);
    }

    public async Task AddRangeAsync(IEnumerable<LedgerEntry> entries)
    {
        await _context.LedgerEntries.AddRangeAsync(entries);
    }

    public async Task<IEnumerable<LedgerEntry>> GetByAccountIdAsync(Guid accountId, int page, int limit)
    {
        return await _context.LedgerEntries
            .Where(e => e.AccountId == accountId)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }
}
