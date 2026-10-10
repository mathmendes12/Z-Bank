using System;

namespace ZBank.Domain.Auth;

public class Device
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string PublicKey { get; private set; }
    public string DeviceName { get; private set; }
    public bool IsTrusted { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Device() { }

    public Device(Guid userId, string publicKey, string deviceName, bool isTrusted = false)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PublicKey = publicKey;
        DeviceName = deviceName;
        IsTrusted = isTrusted;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsTrusted()
    {
        IsTrusted = true;
    }
}
