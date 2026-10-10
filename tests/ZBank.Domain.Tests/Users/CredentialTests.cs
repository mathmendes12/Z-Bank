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
    }
}
