using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IRefreshSessionStore
{
    Task<bool> TryAddAsync(RefreshSession session, CancellationToken cancellationToken);

    Task<RefreshSession?> TryConsumeAsync(
        string tokenDigest,
        int? expectedAccountId,
        DateTimeOffset instantUtc,
        CancellationToken cancellationToken);
}