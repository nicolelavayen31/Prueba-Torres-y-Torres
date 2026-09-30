using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Xunit;

namespace AuthService.UnitTest;

public sealed class InMemoryRefreshSessionStoreTests
{
    [Fact]
    public async Task TryConsumeAsync_AllowsOnlyOneConcurrentUse()
    {
        var store = new InMemoryRefreshSessionStore();
        var now = DateTimeOffset.UtcNow;
        var session = RefreshSession.Start(1, "opaque-token-digest", now, now.AddDays(1));
        await store.TryAddAsync(session, CancellationToken.None);

        var attempts = await Task.WhenAll(
            Enumerable.Range(0, 2).Select(attemptNumber => store.TryConsumeAsync(
                session.TokenDigest,
                null,
                now,
                CancellationToken.None)));

        Assert.Single(attempts, attempt => attempt is not null);
    }
}