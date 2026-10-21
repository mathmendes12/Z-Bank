using System;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using ZBank.Infrastructure.Auth;

namespace ZBank.IntegrationTests.Infrastructure;

public class CryptoServiceTests
{
    private readonly CryptoService _cryptoService = new();

    [Fact]
    public void GenerateNonce_Should_Return_32_Byte_Base64_String()
    {
        // Act
        var nonce = _cryptoService.GenerateNonce();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(nonce));
        var bytes = Convert.FromBase64String(nonce);
        Assert.Equal(32, bytes.Length);
    }

    [Fact]
    public void VerifySignature_Should_Return_True_For_Valid_ECDsa_P256_Signature()
    {
        // Arrange
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var publicKeyBytes = ecdsa.ExportSubjectPublicKeyInfo();
        var publicKeyBase64 = Convert.ToBase64String(publicKeyBytes);

        var nonce = _cryptoService.GenerateNonce();
        var nonceBytes = Encoding.UTF8.GetBytes(nonce);
        var signatureBytes = ecdsa.SignData(nonceBytes, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        var signatureBase64 = Convert.ToBase64String(signatureBytes);

        // Act
        var isValid = _cryptoService.VerifySignature(publicKeyBase64, nonce, signatureBase64);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifySignature_Should_Return_False_For_Tampered_Nonce()
    {
        // Arrange
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var publicKeyBase64 = Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo());

        var nonce = _cryptoService.GenerateNonce();
        var nonceBytes = Encoding.UTF8.GetBytes(nonce);
        var signatureBase64 = Convert.ToBase64String(
            ecdsa.SignData(nonceBytes, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation));

        // Act
        var isValid = _cryptoService.VerifySignature(publicKeyBase64, "tampered-nonce", signatureBase64);

        // Assert
        Assert.False(isValid);
    }
}
