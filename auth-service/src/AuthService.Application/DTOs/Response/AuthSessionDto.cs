namespace AuthService.Application.DTOs.Response;

public sealed record AuthSessionDto(
    int AccountId,
    string Email,
    string DisplayName,
    string AccessToken,
    DateTimeOffset AccessExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshExpiresAtUtc);