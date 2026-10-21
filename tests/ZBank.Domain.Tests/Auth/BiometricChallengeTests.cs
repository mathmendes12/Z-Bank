using System;
using Xunit;
using ZBank.Domain.Auth;

namespace ZBank.Domain.Tests.Auth;

public class BiometricChallengeTests
{
    [Fact]
    public void Create_Should_Initialize_Active_Challenge()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var nonce = "dGVzdC1ub25jZS0xMjM0NQ==";
        var expiresAt = DateTime.UtcNow.AddMinutes(2);

        // Act
        var challenge = new BiometricChallenge(deviceId, nonce, expiresAt);

        // Assert
        Assert.NotEqual(Guid.Empty, challenge.Id);
        Assert.Equal(deviceId, challenge.DeviceId);
        Assert.Equal(nonce, challenge.Nonce);
        Assert.False(challenge.IsUsed);
        Assert.True(challenge.IsValid());
    }

    [Fact]
    public void MarkAsUsed_Should_Set_IsUsed_True_And_Invalidate()
    {
        // Arrange
        var challenge = new BiometricChallenge(Guid.NewGuid(), "nonce", DateTime.UtcNow.AddMinutes(2));

        // Act
        challenge.MarkAsUsed();

        // Assert
        Assert.True(challenge.IsUsed);
        Assert.False(challenge.IsValid());
    }

    [Fact]
    public void MarkAsUsed_Twice_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var challenge = new BiometricChallenge(Guid.NewGuid(), "nonce", DateTime.UtcNow.AddMinutes(2));
        challenge.MarkAsUsed();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => challenge.MarkAsUsed());
    }

    [Fact]
    public void IsValid_Should_Return_False_When_Expired()
    {
        // Arrange
        var challenge = new BiometricChallenge(Guid.NewGuid(), "nonce", DateTime.UtcNow.AddMinutes(-1));

        // Act & Assert
        Assert.False(challenge.IsValid());
    }
}
