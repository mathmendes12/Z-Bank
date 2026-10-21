using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ZBank.Application.Auth;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Auth;
using ZBank.Domain.Users;

namespace ZBank.Application.Tests.Auth;

public class BiometricLoginUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IDeviceRepository> _deviceRepoMock = new();
    private readonly Mock<IBiometricChallengeRepository> _challengeRepoMock = new();
    private readonly Mock<ICryptoService> _cryptoServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<ISessionRepository> _sessionRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly BiometricLoginUseCase _useCase;

    public BiometricLoginUseCaseTests()
    {
        _useCase = new BiometricLoginUseCase(
            _userRepoMock.Object,
            _deviceRepoMock.Object,
            _challengeRepoMock.Object,
            _cryptoServiceMock.Object,
            _tokenServiceMock.Object,
            _sessionRepoMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_Should_Fail_When_Challenge_Is_Invalid_Or_Missing()
    {
        // Arrange
        var cpf = "12345678901";
        var user = User.Create(cpf);
        var deviceId = Guid.NewGuid();
        var device = new Device(user.Id, "pub_key", "Pixel", isTrusted: true);

        _userRepoMock.Setup(r => r.GetByCpfAsync(cpf)).ReturnsAsync(user);
        _deviceRepoMock.Setup(r => r.GetByUserIdAndIdAsync(user.Id, deviceId)).ReturnsAsync(device);
        _challengeRepoMock.Setup(r => r.GetValidChallengeAsync(deviceId, "stale_nonce")).ReturnsAsync((BiometricChallenge?)null);

        var request = new BiometricLoginRequest
        {
            Cpf = cpf,
            DeviceId = deviceId,
            Nonce = "stale_nonce",
            Signature = "sig"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(request));
        _uowMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Mark_Challenge_Used_And_Return_Tokens_When_Valid()
    {
        // Arrange
        var cpf = "12345678901";
        var user = User.Create(cpf);
        var deviceId = Guid.NewGuid();
        var device = new Device(user.Id, "pub_key", "Pixel", isTrusted: true);
        var challenge = new BiometricChallenge(deviceId, "valid_nonce", DateTime.UtcNow.AddMinutes(2));

        _userRepoMock.Setup(r => r.GetByCpfAsync(cpf)).ReturnsAsync(user);
        _deviceRepoMock.Setup(r => r.GetByUserIdAndIdAsync(user.Id, deviceId)).ReturnsAsync(device);
        _challengeRepoMock.Setup(r => r.GetValidChallengeAsync(deviceId, "valid_nonce")).ReturnsAsync(challenge);
        _cryptoServiceMock.Setup(c => c.VerifySignature("pub_key", "valid_nonce", "valid_sig")).Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateTokens(user.Id, device.Id)).Returns(("acc_token", "ref_token"));
        _tokenServiceMock.Setup(t => t.HashRefreshToken("ref_token")).Returns("token_hash");

        var request = new BiometricLoginRequest
        {
            Cpf = cpf,
            DeviceId = deviceId,
            Nonce = "valid_nonce",
            Signature = "valid_sig"
        };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("acc_token", result.AccessToken);
        Assert.True(challenge.IsUsed); // Anti-replay: challenge consumed!
        _uowMock.Verify(u => u.CommitAsync(), Times.Once);
    }
}
