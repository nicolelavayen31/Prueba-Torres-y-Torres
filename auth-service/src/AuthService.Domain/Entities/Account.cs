using AuthService.Domain.ValueObjects;

namespace AuthService.Domain.Entities;

public sealed class Account
{
    private Account(int id, EmailAddress email, string displayName, string passwordHash, DateTimeOffset createdAtUtc)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        CreatedAtUtc = createdAtUtc;
    }

    public int Id { get; }
    public EmailAddress Email { get; }
    public string DisplayName { get; }
    public string PasswordHash { get; }
    public DateTimeOffset CreatedAtUtc { get; }

    public static Account Register(
        EmailAddress email,
        string displayName,
        string passwordHash,
        DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Trim().Length > 80)
        {
            throw new ArgumentException("El nombre debe tener entre 1 y 80 caracteres.", nameof(displayName));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("La credencial protegida es obligatoria.", nameof(passwordHash));
        }

        return new Account(0, email, displayName.Trim(), passwordHash, createdAtUtc);
    }

    public Account WithId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "El ID persistido debe ser positivo.");
        }

        return new Account(id, Email, DisplayName, PasswordHash, CreatedAtUtc);
    }
}