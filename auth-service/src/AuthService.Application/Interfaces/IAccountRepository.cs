using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IAccountRepository
{
    Task<Account?> TryAddAsync(Account account, CancellationToken cancellationToken);
}