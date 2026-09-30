using AuthService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Notifications;

public sealed class LoggingAccountNotifier(ILogger<LoggingAccountNotifier> logger) : IAccountNotifier
{
    public Task SendWelcomeAsync(string email, string displayName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Welcome notification skipped: no email provider is configured.");
        return Task.CompletedTask;
    }
}