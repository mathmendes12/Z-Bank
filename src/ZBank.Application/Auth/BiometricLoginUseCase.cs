using System;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Auth;

namespace ZBank.Application.Auth;

public class BiometricLoginRequest
{
    public string Cpf { get; set; } = string.Empty;
    public Guid DeviceId { get; set; }
    public string Nonce { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

public class BiometricLoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IBiometricChallengeRepository _challengeRepository;
    private readonly ICryptoService _cryptoService;
    private readonly ITokenService _tokenService;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUnitOfWork _uow;

    public BiometricLoginUseCase(
        IUserRepository userRepository,
        IDeviceRepository deviceRepository,
        IBiometricChallengeRepository challengeRepository,
        ICryptoService cryptoService,
        ITokenService tokenService,
        ISessionRepository sessionRepository,
        IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _deviceRepository = deviceRepository;
        _challengeRepository = challengeRepository;
        _cryptoService = cryptoService;
        _tokenService = tokenService;
        _sessionRepository = sessionRepository;
        _uow = uow;
    }

    public async Task<LoginResponse> ExecuteAsync(BiometricLoginRequest request)
    {
        var user = await _userRepository.GetByCpfAsync(request.Cpf);
        if (user == null) throw new UnauthorizedAccessException("Usuário inválido.");

        var device = await _deviceRepository.GetByUserIdAndIdAsync(user.Id, request.DeviceId);
        if (device == null || !device.IsTrusted) throw new UnauthorizedAccessException("Dispositivo não encontrado ou não confiável.");

        var challenge = await _challengeRepository.GetValidChallengeAsync(request.DeviceId, request.Nonce);
        if (challenge == null || !challenge.IsValid())
            throw new UnauthorizedAccessException("Desafio biométrico inválido ou expirado.");

        challenge.MarkAsUsed();

        var isValidSignature = _cryptoService.VerifySignature(device.PublicKey, request.Nonce, request.Signature);
        if (!isValidSignature) throw new UnauthorizedAccessException("Assinatura biométrica inválida.");

        var (accessToken, refreshToken) = _tokenService.GenerateTokens(user.Id, device.Id);
        
        var session = new Session(user.Id, device.Id, _tokenService.HashRefreshToken(refreshToken), DateTime.UtcNow.AddDays(7));
        await _sessionRepository.AddAsync(session);
        await _uow.CommitAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            DeviceId = device.Id
        };
    }
}
