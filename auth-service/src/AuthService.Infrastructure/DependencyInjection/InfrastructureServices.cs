using System.Text;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Application.Validators;
using AuthService.Domain.Interfaces;
using AuthService.Domain.Services;
using AuthService.Infrastructure.Notifications;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.DependencyInjection;

public static class InfrastructureServices
{
    public static IServiceCollection AddAuthServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Issuer) &&
                !string.IsNullOrWhiteSpace(options.Audience) &&
                Encoding.UTF8.GetByteCount(options.SigningKey) >= 32 &&
                options.LifetimeMinutes is > 0 and <= 1440 &&
                options.RefreshLifetimeDays is > 0 and <= 365,
                "La configuración JWT requiere emisor, audiencia, una clave de al menos 32 bytes y duraciones válidas.")
            .ValidateOnStart();

        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<SqlConnectionFactory>();
        services.AddScoped<SqlAccountStore>();
        services.AddScoped<IAccountDirectory>(provider => provider.GetRequiredService<SqlAccountStore>());
        services.AddScoped<IAccountRepository>(provider => provider.GetRequiredService<SqlAccountStore>());
        services.AddScoped<IRefreshSessionStore, SqlRefreshSessionStore>();
        services.AddSingleton<IPasswordPolicy, PasswordPolicy>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddSingleton<IRefreshTokenIssuer, RefreshTokenIssuer>();
        services.AddSingleton<IAccountNotifier, LoggingAccountNotifier>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<AuthenticationRequestValidator>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}