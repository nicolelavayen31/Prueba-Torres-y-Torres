using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Security;

public sealed class RefreshTokenIssuer(IOptions<JwtOptions> options) : IRefreshTokenIssuer
{
    private readonly JwtOptions _options = options.Value;

    public RefreshTokenMaterial Create(DateTimeOffset issuedAtUtc)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var value = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return new RefreshTokenMaterial(
            value,
            ComputeDigest(value),
            issuedAtUtc.AddDays(_options.RefreshLifetimeDays));
    }

    public string ComputeDigest(string refreshToken)
    {
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(digest);
    }
}