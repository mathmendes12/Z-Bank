using System;
using Xunit;
using ZBank.Domain.Invitations;

namespace ZBank.Domain.Tests.Invitations;

public class InvitationTests
{
    [Fact]
    public void Create_Should_Initialize_With_Pending_Status()
    {
        // Act
        var invitation = new Invitation("INV-12345");

        // Assert
        Assert.Equal("INV-12345", invitation.Code);
        Assert.Equal(InvitationStatus.Pending, invitation.Status);
    }

    [Fact]
    public void Use_Should_Change_Status_To_Used()
    {
        // Arrange
        var invitation = new Invitation("INV-12345");

        // Act
        invitation.Use();

        // Assert
        Assert.Equal(InvitationStatus.Used, invitation.Status);
    }

    [Fact]
    public void Use_Should_Throw_When_Already_Used()
    {
        // Arrange
        var invitation = new Invitation("INV-12345");
        invitation.Use();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => invitation.Use());
        Assert.Equal("Invitation has already been used.", ex.Message);
    }
}
