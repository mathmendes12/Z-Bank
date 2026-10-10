using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZBank.Domain.Invitations;
using ZBank.Infrastructure.Data;

namespace ZBank.IntegrationTests.Auth;

public class RegisterUserTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_Should_Return_200_And_Create_User_With_UnderAnalysis_Status()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var invitation = new Invitation("REG-CODE-123");
        context.Invitations.Add(invitation);
        await context.SaveChangesAsync();

        using var client = Factory.CreateClient();

        // Act
        var request = new
        {
            InvitationCode = "REG-CODE-123",
            Cpf = "12345678901",
            Password = "MySecurePassword123"
        };
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedInvitation = await context.Invitations.FirstOrDefaultAsync(i => i.Code == "REG-CODE-123");
        Assert.Equal(InvitationStatus.Used, updatedInvitation!.Status);

        var user = await context.Users.FirstOrDefaultAsync(u => u.Cpf == "12345678901");
        Assert.NotNull(user);
        Assert.Equal(Domain.Users.UserStatus.UnderAnalysis, user.Status);

        var credential = await context.Credentials.FirstOrDefaultAsync(c => c.UserId == user.Id);
        Assert.NotNull(credential);
        Assert.NotEmpty(credential.PasswordHash);
    }
}
