using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ZBank.Application.Common.Interfaces;

namespace ZBank.Infrastructure.Auth;

public class TokenService : ITokenService
{
    private const string SecretKey = "zbank-super-secret-key-for-development-purposes";
    private readonly SymmetricSecurityKey _key;

    public TokenService()
    {
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey.PadRight(32, '0')));
    }

    public (string AccessToken, string RefreshToken) GenerateTokens(Guid userId, Guid deviceId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("DeviceId", deviceId.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(token);

        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);

        return (accessToken, refreshToken);
    }

    public string HashRefreshToken(string refreshToken)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
