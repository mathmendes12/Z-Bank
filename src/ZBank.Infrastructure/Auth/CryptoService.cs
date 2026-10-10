using System;
using System.Security.Cryptography;
using System.Text;
using ZBank.Application.Common.Interfaces;

namespace ZBank.Infrastructure.Auth;

public class CryptoService : ICryptoService
{
    public string GenerateNonce()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public bool VerifySignature(string publicKeyBase64, string nonce, string signatureBase64)
    {
        try
        {
            var publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
            var signatureBytes = Convert.FromBase64String(signatureBase64);
            var nonceBytes = Encoding.UTF8.GetBytes(nonce);

            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            
            return ecdsa.VerifyData(nonceBytes, signatureBytes, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        }
        catch
        {
            try
            {
                // Fallback for RSA if needed, or different format
                var publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
                var signatureBytes = Convert.FromBase64String(signatureBase64);
                var nonceBytes = Encoding.UTF8.GetBytes(nonce);

                using var rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
                return rsa.VerifyData(nonceBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch
            {
                return false;
            }
        }
    }
}
