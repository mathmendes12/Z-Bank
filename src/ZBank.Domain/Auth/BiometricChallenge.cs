using System;

namespace ZBank.Domain.Auth;

public class BiometricChallenge
{
    public Guid Id { get; private set; }
    public Guid DeviceId { get; private set; }
    public string Nonce { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    private BiometricChallenge() { Nonce = null!; } // EF Core

    public BiometricChallenge(Guid deviceId, string nonce, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        DeviceId = deviceId;
        Nonce = nonce;
        ExpiresAt = expiresAt;
        IsUsed = false;
    }

    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new InvalidOperationException("Challenge has already been used.");

        IsUsed = true;
    }

    public bool IsValid()
    {
        return !IsUsed && ExpiresAt > DateTime.UtcNow;
    }
}
