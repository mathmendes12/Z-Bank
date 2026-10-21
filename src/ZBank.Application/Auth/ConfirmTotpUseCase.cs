using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;

namespace ZBank.Application.Auth;

public record ConfirmTotpRequest(string TotpCode);

public record ConfirmTotpResponse(bool IsSuccess, string? ErrorMessage = null);

public class ConfirmTotpUseCase
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly ITotpService _totpService;
    private readonly IUnitOfWork _uow;

    public ConfirmTotpUseCase(ICredentialRepository credentialRepository, ITotpService totpService, IUnitOfWork uow)
    {
        _credentialRepository = credentialRepository;
        _totpService = totpService;
        _uow = uow;
    }

    public async Task<ConfirmTotpResponse> ExecuteAsync(Guid userId, ConfirmTotpRequest request)
    {
        var credential = await _credentialRepository.GetByUserIdAsync(userId);
        if (credential == null)
            throw new KeyNotFoundException("User credentials not found.");

        if (string.IsNullOrWhiteSpace(credential.TotpSecret))
            throw new InvalidOperationException("TOTP setup has not been initiated.");

        if (!_totpService.VerifyCode(credential.TotpSecret, request.TotpCode))
            throw new UnauthorizedAccessException("Código TOTP inválido.");

        credential.ConfirmTotp();
        await _uow.CommitAsync();

        return new ConfirmTotpResponse(true);
    }
}
