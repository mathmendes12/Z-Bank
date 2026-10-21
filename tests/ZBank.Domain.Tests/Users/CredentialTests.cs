using System;
using Xunit;
using ZBank.Domain.Users;

namespace ZBank.Domain.Tests.Users;

public class CredentialTests
{
    [Fact]
    public void Create_Should_Initialize_With_Correct_Properties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var passwordHash = "$argon2id$v=19$m=65536,t=3,p=4$somehash";

        // Act
        var credential = new Credential(userId, passwordHash);

        // Assert
        Assert.NotEqual(Guid.Empty, credential.Id);
        Assert.Equal(userId, credential.UserId);
        Assert.Equal(passwordHash, credential.PasswordHash);
        Assert.False(credential.TotpEnabled);
        Assert.Null(credential.TotpSecret);
    }

    [Fact]
    public void SetTotpSecret_Should_Set_Secret_Without_Enabling_Totp()
    {
        // Arrange
        var credential = new Credential(Guid.NewGuid(), "hash");
        var secret = "JBSWY3DPEHPK3PXP";

        // Act
        credential.SetTotpSecret(secret);

        // Assert
        Assert.Equal(secret, credential.TotpSecret);
        Assert.False(credential.TotpEnabled);
    }

    [Fact]
    public void ConfirmTotp_Should_Enable_Totp_When_Secret_Is_Present()
    {
        // Arrange
        var credential = new Credential(Guid.NewGuid(), "hash");
        credential.SetTotpSecret("JBSWY3DPEHPK3PXP");

        // Act
        credential.ConfirmTotp();

        // Assert
        Assert.True(credential.TotpEnabled);
    }

    [Fact]
    public void ConfirmTotp_Should_Throw_When_Secret_Is_Missing()
    {
        // Arrange
        var credential = new Credential(Guid.NewGuid(), "hash");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => credential.ConfirmTotp());
    }
}
