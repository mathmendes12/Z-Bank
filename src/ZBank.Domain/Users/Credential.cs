using System;

namespace ZBank.Domain.Users;

public class Credential
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string PasswordHash { get; private set; }
    public string? TotpSecret { get; private set; }
    public bool TotpEnabled { get; private set; }

    private Credential() { PasswordHash = null!; } // EF Core

    public Credential(Guid userId, string passwordHash)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PasswordHash = passwordHash;
        TotpEnabled = false;
    }

    public void SetTotpSecret(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("TOTP Secret cannot be empty", nameof(secret));

        TotpSecret = secret;
        TotpEnabled = false;
    }

    public void ConfirmTotp()
    {
        if (string.IsNullOrWhiteSpace(TotpSecret))
            throw new InvalidOperationException("Cannot confirm TOTP without a secret.");

        TotpEnabled = true;
    }

    public void EnableTotp(string secret)
    {
        SetTotpSecret(secret);
        ConfirmTotp();
    }
}
