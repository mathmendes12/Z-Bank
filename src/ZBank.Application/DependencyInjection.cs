using Microsoft.Extensions.DependencyInjection;
using ZBank.Application.Auth;
using ZBank.Application.Invitations;

namespace ZBank.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ValidateInvitationUseCase>();
        services.AddScoped<RegisterUserUseCase>();
        services.AddScoped<SetupTotpUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<RefreshTokensUseCase>();
        services.AddScoped<RegisterDeviceUseCase>();
        services.AddScoped<BiometricChallengeUseCase>();
        services.AddScoped<BiometricLoginUseCase>();

        return services;
    }
}
