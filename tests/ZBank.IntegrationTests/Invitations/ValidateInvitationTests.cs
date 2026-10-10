using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZBank.Domain.Invitations;
using ZBank.Infrastructure.Data;

namespace ZBank.IntegrationTests.Invitations;

public class ValidateInvitationTests : IntegrationTestBase
{
    [Fact]
    public async Task Validate_Should_Return_200_When_Invitation_Is_Valid()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var invitation = new Invitation("VALID-CODE-123");
        context.Invitations.Add(invitation);
        await context.SaveChangesAsync();

        using var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/invitations/validate", new { Code = "VALID-CODE-123" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Validate_Should_Return_400_When_Invitation_Is_Invalid()
    {
        using var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/invitations/validate", new { Code = "INVALID-CODE" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
