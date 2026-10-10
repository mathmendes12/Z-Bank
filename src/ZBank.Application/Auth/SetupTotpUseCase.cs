using System;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;

namespace ZBank.Application.Auth;

public class SetupTotpRequest
{
    public Guid UserId { get; set; }
}

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

    public async Task<SetupTotpResponse> ExecuteAsync(SetupTotpRequest request)
    {
        var credential = await _credentialRepository.GetByUserIdAsync(request.UserId);
        if (credential == null) throw new Exception("User credentials not found");
        
        var secret = _totpService.GenerateSecret();
        credential.EnableTotp(secret);
        
        await _uow.CommitAsync();
        
        return new SetupTotpResponse { TotpSecret = secret };
    }
}
