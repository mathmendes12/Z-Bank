using System;

namespace ZBank.Domain.Auth;

public class Session
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid DeviceId { get; private set; }
    public string RefreshTokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private Session() { }

    public Session(Guid userId, Guid deviceId, string refreshTokenHash, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        DeviceId = deviceId;
        RefreshTokenHash = refreshTokenHash;
        ExpiresAt = expiresAt;
        IsRevoked = false;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }

    public void UpdateRefreshToken(string newHash, DateTime newExpiresAt)
    {
        RefreshTokenHash = newHash;
        ExpiresAt = newExpiresAt;
    }

    public bool IsValid()
    {
        return !IsRevoked && ExpiresAt > DateTime.UtcNow;
    }
}
