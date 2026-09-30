namespace AuthService.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    public string Issuer { get; init; } = "AuthService";
    public string Audience { get; init; } = "AuthService.Client";
    public string SigningKey { get; init; } = string.Empty;
    public int LifetimeMinutes { get; init; } = 20;
    public int RefreshLifetimeDays { get; init; } = 14;
}