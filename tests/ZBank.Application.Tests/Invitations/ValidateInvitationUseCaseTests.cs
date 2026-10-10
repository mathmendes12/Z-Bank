using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ZBank.Application.Invitations;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Invitations;

namespace ZBank.Application.Tests.Invitations;

public class ValidateInvitationUseCaseTests
{
    private readonly Mock<IInvitationRepository> _invitationRepositoryMock;
    private readonly ValidateInvitationUseCase _useCase;

    public ValidateInvitationUseCaseTests()
    {
        _invitationRepositoryMock = new Mock<IInvitationRepository>();
        _useCase = new ValidateInvitationUseCase(_invitationRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_True_When_Invitation_Is_Pending()
    {
        // Arrange
        var code = "INV-VALID";
        var invitation = new Invitation(code);
        _invitationRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(code))
            .ReturnsAsync(invitation);

        // Act
        var result = await _useCase.ExecuteAsync(code);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_False_When_Invitation_Is_Used()
    {
        // Arrange
        var code = "INV-USED";
        var invitation = new Invitation(code);
        invitation.Use();
        _invitationRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(code))
            .ReturnsAsync(invitation);

        // Act
        var result = await _useCase.ExecuteAsync(code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_False_When_Invitation_Does_Not_Exist()
    {
        // Arrange
        var code = "INV-UNKNOWN";
        _invitationRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(code))
            .ReturnsAsync((Invitation?)null);

        // Act
        var result = await _useCase.ExecuteAsync(code);

        // Assert
        Assert.False(result);
    }
}
