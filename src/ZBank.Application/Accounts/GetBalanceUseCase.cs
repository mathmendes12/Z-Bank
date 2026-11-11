using System;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;

namespace ZBank.Application.Accounts;

public class AccountBalanceResponse
{
    public long Balance { get; set; }
}

public interface IGetBalanceUseCase
{
    Task<AccountBalanceResponse?> ExecuteAsync(Guid userId);
}

public class GetBalanceUseCase : IGetBalanceUseCase
{
    private readonly IAccountRepository _accountRepository;

    public GetBalanceUseCase(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<AccountBalanceResponse?> ExecuteAsync(Guid userId)
    {
        var account = await _accountRepository.GetByUserIdAsync(userId);
        if (account == null)
            return null;

        return new AccountBalanceResponse { Balance = account.Balance };
    }
}
