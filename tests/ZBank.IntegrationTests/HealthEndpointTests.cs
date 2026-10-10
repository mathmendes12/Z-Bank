using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ZBank.IntegrationTests;

public class HealthEndpointTests
{
    // Prova de ponta a ponta do critério de pronto da Fase 0: a API sobe via
    // WebApplicationFactory e /health responde 200. É este endpoint que o app mobile chama pelo
    // túnel pra provar que o backend está vivo.
    [Fact]
    public async Task Health_retorna_200()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
