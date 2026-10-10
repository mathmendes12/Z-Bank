var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

// Prova de vida pro critério de pronto da Fase 0: o app mobile chama essa rota pelo túnel.
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ZBank.Api.Customer" }));

app.Run();

// Classe parcial pública pra WebApplicationFactory<Program> funcionar nos testes de integração
// (ZBank.IntegrationTests) sem precisar expor mais do que isso.
public partial class Program { }
