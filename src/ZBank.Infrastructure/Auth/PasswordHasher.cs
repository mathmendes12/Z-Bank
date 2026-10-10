using System.Text;
using Isopoh.Cryptography.Argon2;
using ZBank.Application.Common.Interfaces;

namespace ZBank.Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        // Using recommended OWASP settings or defaults
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var salt = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(salt);
        
        var config = new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing, // Argon2id
            Version = Argon2Version.Nineteen,
            TimeCost = 3,
            MemoryCost = 65536,
            Lanes = 4,
            Threads = Environment.ProcessorCount,
            Password = passwordBytes,
            Salt = salt,
            HashLength = 32
        };

        using var argon2 = new Argon2(config);
        using var hash = argon2.Hash();

        return config.EncodeString(hash.Buffer);
    }

    public bool Verify(string hash, string password)
    {
        return Argon2.Verify(hash, password);
    }
}
