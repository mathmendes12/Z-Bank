using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ZBank.Application.Auth;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Users;

namespace ZBank.Application.Tests.Auth;

public class ConfirmTotpUseCaseTests
{
    private readonly Mock<ICredentialRepository> _credentialRepoMock;
    private readonly Mock<ITotpService> _totpServiceMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly ConfirmTotpUseCase _useCase;

    public ConfirmTotpUseCaseTests()
    {
        _credentialRepoMock = new Mock<ICredentialRepository>();
        _totpServiceMock = new Mock<ITotpService>();
        _uowMock = new Mock<IUnitOfWork>();

        _useCase = new ConfirmTotpUseCase(
            _credentialRepoMock.Object,
            _totpServiceMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_Should_Confirm_Totp_When_Code_Is_Valid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var credential = new Credential(userId, "hashed_password");
        credential.SetTotpSecret("SECRET123");

        _credentialRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(credential);
        _totpServiceMock.Setup(s => s.VerifyCode("SECRET123", "123456")).Returns(true);

        // Act
        var result = await _useCase.ExecuteAsync(userId, new ConfirmTotpRequest("123456"));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(credential.TotpEnabled);
        _uowMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Throw_When_Code_Is_Invalid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var credential = new Credential(userId, "hashed_password");
        credential.SetTotpSecret("SECRET123");

        _credentialRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(credential);
        _totpServiceMock.Setup(s => s.VerifyCode("SECRET123", "999999")).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _useCase.ExecuteAsync(userId, new ConfirmTotpRequest("999999")));

        Assert.False(credential.TotpEnabled);
        _uowMock.Verify(u => u.CommitAsync(), Times.Never);
    }
}
