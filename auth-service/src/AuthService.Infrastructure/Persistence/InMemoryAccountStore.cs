using System.Collections.Concurrent;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Infrastructure.Persistence;

public sealed class InMemoryAccountStore : IAccountDirectory, IAccountRepository
{
    private readonly ConcurrentDictionary<string, Account> _accounts = new(StringComparer.Ordinal);
    private int _lastAssignedId;

    public Task<Account?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _accounts.TryGetValue(email, out var account);
        return Task.FromResult(account);
    }

    public Task<Account?> FindByIdAsync(int accountId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var account = _accounts.Values.FirstOrDefault(candidate => candidate.Id == accountId);
        return Task.FromResult(account);
    }

    public Task<Account?> TryAddAsync(Account account, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var persistedAccount = account.WithId(Interlocked.Increment(ref _lastAssignedId));
        return Task.FromResult(_accounts.TryAdd(account.Email.Value, persistedAccount) ? persistedAccount : null);
    }
}