using AuthService.Application.DTOs.Response;
using AuthService.Domain.Entities;

namespace AuthService.Application.Mappings;

public static class AuthSessionMapper
{
    public static AuthSessionDto ToDto(
        Account account,
        IssuedAccessToken accessToken,
        string refreshToken,
        DateTimeOffset refreshExpiresAtUtc) =>
        new(
            account.Id,
            account.Email.Value,
            account.DisplayName,
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            refreshToken,
            refreshExpiresAtUtc);
}