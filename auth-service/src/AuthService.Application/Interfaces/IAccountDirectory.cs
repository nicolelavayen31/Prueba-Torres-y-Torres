using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IAccountDirectory
{
    Task<Account?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Account?> FindByIdAsync(int accountId, CancellationToken cancellationToken);
}