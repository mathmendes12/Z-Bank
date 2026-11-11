using Microsoft.Extensions.DependencyInjection;
using ZBank.Application.Auth;
using ZBank.Application.Invitations;
using ZBank.Application.Ledger;
using ZBank.Application.Accounts;

namespace ZBank.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ValidateInvitationUseCase>();
        services.AddScoped<RegisterUserUseCase>();
        services.AddScoped<SetupTotpUseCase>();
        services.AddScoped<ConfirmTotpUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<RefreshTokensUseCase>();
        services.AddScoped<RegisterDeviceUseCase>();
        services.AddScoped<BiometricChallengeUseCase>();
        services.AddScoped<BiometricLoginUseCase>();
        
        services.AddScoped<ILedgerTransactionService, LedgerTransactionService>();
        services.AddScoped<IGetBalanceUseCase, GetBalanceUseCase>();
        services.AddScoped<IGetStatementUseCase, GetStatementUseCase>();

        return services;
    }
}
