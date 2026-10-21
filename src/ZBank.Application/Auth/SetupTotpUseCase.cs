using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;

namespace ZBank.Application.Auth;

public class SetupTotpResponse
{
    public string TotpSecret { get; set; } = string.Empty;
}

public class SetupTotpUseCase
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly ITotpService _totpService;
    private readonly IUnitOfWork _uow;

    public SetupTotpUseCase(ICredentialRepository credentialRepository, ITotpService totpService, IUnitOfWork uow)
    {
        _credentialRepository = credentialRepository;
        _totpService = totpService;
        _uow = uow;
    }

    public async Task<SetupTotpResponse> ExecuteAsync(Guid userId)
    {
        var credential = await _credentialRepository.GetByUserIdAsync(userId);
        if (credential == null) throw new KeyNotFoundException("User credentials not found.");
        
        var secret = _totpService.GenerateSecret();
        credential.SetTotpSecret(secret);
        
        await _uow.CommitAsync();
        
        return new SetupTotpResponse { TotpSecret = secret };
    }
}
