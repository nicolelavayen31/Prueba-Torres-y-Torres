namespace AuthService.Application.DTOs.Response;

public sealed record IssuedAccessToken(string Value, DateTimeOffset ExpiresAtUtc);