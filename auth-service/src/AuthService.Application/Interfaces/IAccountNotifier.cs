namespace AuthService.Application.Interfaces;

public interface IAccountNotifier
{
    Task SendWelcomeAsync(string email, string displayName, CancellationToken cancellationToken);
}