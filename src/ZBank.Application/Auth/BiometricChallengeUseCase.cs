using System;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Auth;

namespace ZBank.Application.Auth;

public class BiometricChallengeRequest
{
    public Guid DeviceId { get; set; }
}

public class BiometricChallengeResponse
{
    public string Nonce { get; set; } = string.Empty;
}

public class BiometricChallengeUseCase
{
    private readonly ICryptoService _cryptoService;
    private readonly IBiometricChallengeRepository _challengeRepository;
    private readonly IUnitOfWork _uow;

    public BiometricChallengeUseCase(
        ICryptoService cryptoService,
        IBiometricChallengeRepository challengeRepository,
        IUnitOfWork uow)
    {
        _cryptoService = cryptoService;
        _challengeRepository = challengeRepository;
        _uow = uow;
    }

    public async Task<BiometricChallengeResponse> ExecuteAsync(BiometricChallengeRequest request)
    {
        var nonce = _cryptoService.GenerateNonce();
        var challenge = new BiometricChallenge(request.DeviceId, nonce, DateTime.UtcNow.AddMinutes(2));

        await _challengeRepository.AddAsync(challenge);
        await _uow.CommitAsync();

        return new BiometricChallengeResponse { Nonce = nonce };
    }
}
