using System;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Auth;

namespace ZBank.Application.Auth;

public class LoginRequest
{
    public string Cpf { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TotpCode { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public Guid DeviceId { get; set; }
}

public class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ICredentialRepository _credentialRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITotpService _totpService;
    private readonly ITokenService _tokenService;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUnitOfWork _uow;

    public LoginUseCase(
        IUserRepository userRepository,
        ICredentialRepository credentialRepository,
        IPasswordHasher passwordHasher,
        ITotpService totpService,
        ITokenService tokenService,
        ISessionRepository sessionRepository,
        IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _credentialRepository = credentialRepository;
        _passwordHasher = passwordHasher;
        _totpService = totpService;
        _tokenService = tokenService;
        _sessionRepository = sessionRepository;
        _uow = uow;
    }

    public async Task<LoginResponse> ExecuteAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByCpfAsync(request.Cpf);
        if (user == null) throw new Exception("Invalid credentials");

        var credential = await _credentialRepository.GetByUserIdAsync(user.Id);
        if (credential == null) throw new Exception("Invalid credentials");

        if (!_passwordHasher.Verify(credential.PasswordHash, request.Password))
            throw new Exception("Invalid credentials");

        if (!credential.TotpEnabled || string.IsNullOrEmpty(credential.TotpSecret) || !_totpService.VerifyCode(credential.TotpSecret, request.TotpCode))
            throw new Exception("Invalid TOTP code");

        // Temporary DeviceId for standard login (Biometrics will register a real device later, or use this one)
        var deviceId = Guid.NewGuid();
        
        var (accessToken, refreshToken) = _tokenService.GenerateTokens(user.Id, deviceId);
        
        var session = new Session(user.Id, deviceId, _tokenService.HashRefreshToken(refreshToken), DateTime.UtcNow.AddDays(7));
        await _sessionRepository.AddAsync(session);
        await _uow.CommitAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            DeviceId = deviceId
        };
    }
}
