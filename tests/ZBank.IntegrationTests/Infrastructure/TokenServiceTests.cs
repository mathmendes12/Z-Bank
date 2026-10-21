using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Xunit;
using ZBank.Infrastructure.Auth;

namespace ZBank.IntegrationTests.Infrastructure;

public class TokenServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey = "a-very-secure-and-long-secret-key-for-testing-12345";

    public TokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:SecretKey", _secretKey }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public void GenerateTokens_Should_Include_UserId_And_DeviceId_Claims()
    {
        // Arrange
        var tokenService = new TokenService(_configuration);
        var userId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();

        // Act
        var (accessToken, refreshToken) = tokenService.GenerateTokens(userId, deviceId);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshToken));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);

        Assert.Equal(userId.ToString(), jwt.Subject);
        Assert.Contains(jwt.Claims, c => c.Type == "DeviceId" && c.Value == deviceId.ToString());
    }

    [Fact]
    public void HashRefreshToken_Should_Produce_Consistent_Hash()
    {
        // Arrange
        var tokenService = new TokenService(_configuration);
        var token = "some-random-refresh-token";

        // Act
        var hash1 = tokenService.HashRefreshToken(token);
        var hash2 = tokenService.HashRefreshToken(token);

        // Assert
        Assert.Equal(hash1, hash2);
    }
}
