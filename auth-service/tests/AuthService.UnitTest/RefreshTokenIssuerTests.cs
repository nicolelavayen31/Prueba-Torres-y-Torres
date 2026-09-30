using AuthService.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Xunit;

namespace AuthService.UnitTest;

public sealed class RefreshTokenIssuerTests
{
    [Fact]
    public void Create_GeneratesUniqueOpaqueTokensWithConfiguredExpiry()
    {
        var issuer = new RefreshTokenIssuer(Options.Create(new JwtOptions { RefreshLifetimeDays = 14 }));
        var issuedAt = new DateTimeOffset(2026, 9, 29, 12, 0, 0, TimeSpan.Zero);

        var first = issuer.Create(issuedAt);
        var second = issuer.Create(issuedAt);

        Assert.NotEqual(first.Value, second.Value);
        Assert.Equal(issuedAt.AddDays(14), first.ExpiresAtUtc);
        Assert.Equal(issuer.ComputeDigest(first.Value), first.Digest);
        Assert.NotEqual(first.Value, first.Digest);
    }
}