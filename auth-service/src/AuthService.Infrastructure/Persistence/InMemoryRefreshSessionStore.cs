using System.Collections.Concurrent;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Infrastructure.Persistence;

public sealed class InMemoryRefreshSessionStore : IRefreshSessionStore
{
    private readonly ConcurrentDictionary<string, RefreshSession> _sessions = new(StringComparer.Ordinal);

    public Task<bool> TryAddAsync(RefreshSession session, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_sessions.TryAdd(session.TokenDigest, session));
    }

    public Task<RefreshSession?> TryConsumeAsync(
        string tokenDigest,
        int? expectedAccountId,
        DateTimeOffset instantUtc,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        while (_sessions.TryGetValue(tokenDigest, out var session))
        {
            if (!session.IsActiveAt(instantUtc) ||
                expectedAccountId is int accountId && session.AccountId != accountId)
            {
                return Task.FromResult<RefreshSession?>(null);
            }

            if (_sessions.TryUpdate(tokenDigest, session.RevokeAt(instantUtc), session))
            {
                return Task.FromResult<RefreshSession?>(session);
            }
        }

        return Task.FromResult<RefreshSession?>(null);
    }
}