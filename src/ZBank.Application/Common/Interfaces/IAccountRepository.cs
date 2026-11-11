using System;
using System.Threading.Tasks;
using ZBank.Domain.Accounts;

namespace ZBank.Application.Common.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account?> GetByIdForUpdateAsync(Guid id);
    Task<Account?> GetByUserIdAsync(Guid userId);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
}
