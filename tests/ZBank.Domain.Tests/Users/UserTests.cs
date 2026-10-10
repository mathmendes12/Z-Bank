using System;
using Xunit;
using ZBank.Domain.Users;

namespace ZBank.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_Should_Initialize_User_With_UnderAnalysis_Status()
    {
        // Arrange
        var cpf = "12345678901";

        // Act
        var user = User.Create(cpf);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(cpf, user.Cpf);
        Assert.Equal(UserStatus.UnderAnalysis, user.Status);
    }
}
