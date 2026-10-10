using System;
using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Auth;

namespace ZBank.Application.Auth;

public class RefreshTokensRequest
{
    public Guid UserId { get; set; }
    public Guid DeviceId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokensResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokensUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;

    public RefreshTokensUseCase(
        ISessionRepository sessionRepository,
        ITokenService tokenService,
        IUnitOfWork uow)
    {
        _sessionRepository = sessionRepository;
        _tokenService = tokenService;
        _uow = uow;
    }

    public async Task<RefreshTokensResponse> ExecuteAsync(RefreshTokensRequest request)
    {
        var session = await _sessionRepository.GetByDeviceAndUserAsync(request.DeviceId, request.UserId);
        if (session == null) throw new Exception("Session not found");

        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        
        if (session.RefreshTokenHash != tokenHash)
        {
            // Reuse detected
            await _sessionRepository.RevokeAllUserSessionsAsync(request.UserId);
            await _uow.CommitAsync();
            throw new Exception("Compromised session detected. All sessions revoked.");
        }

        if (!session.IsValid())
        {
            throw new Exception("Session expired or revoked");
        }

        var (newAccessToken, newRefreshToken) = _tokenService.GenerateTokens(request.UserId, request.DeviceId);
        
        session.UpdateRefreshToken(_tokenService.HashRefreshToken(newRefreshToken), DateTime.UtcNow.AddDays(7));
        await _uow.CommitAsync();

        return new RefreshTokensResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}
