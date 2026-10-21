using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ZBank.Api.Customer.Middlewares;
using ZBank.Application;
using ZBank.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "zbank-dev-secret-key-32-chars-minimum-length-change-in-prod";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret.PadRight(32, '0')));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Prova de vida pro critério de pronto da Fase 0: o app mobile chama essa rota pelo túnel.
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ZBank.Api.Customer" }));

app.Run();

// Classe parcial pública pra WebApplicationFactory<Program> funcionar nos testes de integração
// (ZBank.IntegrationTests) sem precisar expor mais do que isso.
public partial class Program { }
