using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;
using ZBank.Application.Auth;
using ZBank.Application.Common.Interfaces;

namespace ZBank.IntegrationTests.Auth;

public class DeviceRegistrationAuthorizationTests
{
    [Fact]
    public async Task RegisterDevice_Without_Token_Should_Return_401_Unauthorized()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var payload = new
        {
            publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE...",
            deviceName = "Android Pixel 8"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/devices/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterDevice_With_Valid_Token_Should_Extract_UserId_From_Claims_And_Return_200()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceRepositoryMock = new Mock<IDeviceRepository>();
        var uowMock = new Mock<IUnitOfWork>();

        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => deviceRepositoryMock.Object);
                services.AddScoped(_ => uowMock.Object);
            });
        });

        using var client = factory.CreateClient();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("zbank-dev-secret-key-32-chars-minimum-length-change-in-prod".PadRight(32, '0'));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);

        var payload = new
        {
            publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE...",
            deviceName = "Android Pixel 8"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/devices/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        deviceRepositoryMock.Verify(r => r.AddAsync(It.Is<Domain.Auth.Device>(d => d.UserId == userId)), Times.Once);
        uowMock.Verify(u => u.CommitAsync(), Times.Once);
    }
}
