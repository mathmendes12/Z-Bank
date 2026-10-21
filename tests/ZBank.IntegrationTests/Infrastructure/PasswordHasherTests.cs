using Xunit;
using ZBank.Infrastructure.Auth;

namespace ZBank.IntegrationTests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_Should_Use_Argon2id_Prefix()
    {
        // Arrange
        var password = "SuperSecretPassword123!";

        // Act
        var hash = _hasher.Hash(password);

        // Assert - Argon2id must produce $argon2id$ format, NOT $argon2i$ or $argon2d$
        Assert.StartsWith("$argon2id$", hash);
    }

    [Fact]
    public void Verify_Should_Return_True_For_Correct_Password()
    {
        // Arrange
        var password = "SuperSecretPassword123!";
        var hash = _hasher.Hash(password);

        // Act
        var isValid = _hasher.Verify(hash, password);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Verify_Should_Return_False_For_Incorrect_Password()
    {
        // Arrange
        var password = "SuperSecretPassword123!";
        var hash = _hasher.Hash(password);

        // Act
        var isValid = _hasher.Verify(hash, "WrongPassword!");

        // Assert
        Assert.False(isValid);
    }
}
