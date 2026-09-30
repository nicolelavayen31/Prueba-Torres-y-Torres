using System.Security.Claims;
using System.Globalization;
using AuthService.Application.Interfaces;
using AuthService.Shared.Constants;
using Microsoft.AspNetCore.Http;

namespace AuthService.Infrastructure.Security;

public sealed class HttpCurrentUser(IHttpContextAccessor contextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => contextAccessor.HttpContext?.User;

    public int? AccountId => int.TryParse(
        Principal?.FindFirstValue(JwtClaimNames.AccountId),
        NumberStyles.None,
        CultureInfo.InvariantCulture,
        out var id)
        ? id
        : null;

    public string? Email => Principal?.FindFirstValue(JwtClaimNames.Email);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}