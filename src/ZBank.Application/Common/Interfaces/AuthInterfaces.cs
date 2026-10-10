using System;
using System.Threading.Tasks;
using ZBank.Domain.Users;
using ZBank.Domain.Auth;

namespace ZBank.Application.Common.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByCpfAsync(string cpf);
    Task<User?> GetByIdAsync(Guid id);
}

public interface ICredentialRepository
{
    Task AddAsync(Credential credential);
    Task<Credential?> GetByUserIdAsync(Guid userId);
}

public interface IDeviceRepository
{
    Task AddAsync(Device device);
    Task<Device?> GetByIdAsync(Guid id);
    Task<Device?> GetByUserIdAndIdAsync(Guid userId, Guid deviceId);
}

public interface ISessionRepository
{
    Task AddAsync(Session session);
    Task<Session?> GetByIdAsync(Guid id);
    Task<Session?> GetByDeviceAndUserAsync(Guid deviceId, Guid userId);
    Task RevokeAllUserSessionsAsync(Guid userId);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hash, string password);
}

public interface ITotpService
{
    string GenerateSecret();
    bool VerifyCode(string secret, string code);
}

public interface ITokenService
{
    (string AccessToken, string RefreshToken) GenerateTokens(Guid userId, Guid deviceId);
    string HashRefreshToken(string refreshToken);
}

public interface ICryptoService
{
    string GenerateNonce();
    bool VerifySignature(string publicKey, string nonce, string signature);
}

public interface IUnitOfWork
{
    Task CommitAsync();
}
