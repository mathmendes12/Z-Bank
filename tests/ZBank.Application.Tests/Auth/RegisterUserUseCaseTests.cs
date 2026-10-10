using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ZBank.Application.Auth;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Invitations;
using ZBank.Domain.Users;

namespace ZBank.Application.Tests.Auth;

public class RegisterUserUseCaseTests
{
    private readonly Mock<IInvitationRepository> _invitationRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ICredentialRepository> _credentialRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterUserUseCase _useCase;

    public RegisterUserUseCaseTests()
    {
        _invitationRepoMock = new Mock<IInvitationRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _credentialRepoMock = new Mock<ICredentialRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _useCase = new RegisterUserUseCase(
            _invitationRepoMock.Object,
            _userRepoMock.Object,
            _credentialRepoMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_Should_Register_User_And_Mark_Invitation_As_Used()
    {
        // Arrange
        var request = new RegisterUserRequest("INV-123", "12345678901", "StrongPass!");
        var invitation = new Invitation("INV-123");

        _invitationRepoMock
            .Setup(r => r.GetByCodeAsync(request.InvitationCode))
            .ReturnsAsync(invitation);

        _passwordHasherMock
            .Setup(h => h.Hash(request.Password))
            .Returns("hashed_password");

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        
        Assert.Equal(InvitationStatus.Used, invitation.Status);
        _invitationRepoMock.Verify(r => r.UpdateAsync(invitation), Times.Once);
        _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Cpf == request.Cpf && u.Status == UserStatus.UnderAnalysis)), Times.Once);
        _credentialRepoMock.Verify(r => r.AddAsync(It.Is<Credential>(c => c.PasswordHash == "hashed_password")), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Fail_When_Invitation_Not_Found()
    {
        // Arrange
        var request = new RegisterUserRequest("INV-123", "12345678901", "StrongPass!");

        _invitationRepoMock
            .Setup(r => r.GetByCodeAsync(request.InvitationCode))
            .ReturnsAsync((Invitation?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid or already used invitation.", result.ErrorMessage);
        
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Fail_When_Invitation_Already_Used()
    {
        // Arrange
        var request = new RegisterUserRequest("INV-123", "12345678901", "StrongPass!");
        var invitation = new Invitation("INV-123");
        invitation.Use();

        _invitationRepoMock
            .Setup(r => r.GetByCodeAsync(request.InvitationCode))
            .ReturnsAsync(invitation);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid or already used invitation.", result.ErrorMessage);
        
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }
}
