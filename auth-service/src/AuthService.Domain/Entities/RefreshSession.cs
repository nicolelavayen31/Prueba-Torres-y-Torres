namespace AuthService.Domain.Entities;

public sealed record RefreshSession(
    int AccountId,
    string TokenDigest,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset? RevokedAtUtc)
{
    public static RefreshSession Start(
        int accountId,
        string tokenDigest,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (accountId <= 0)
        {
            throw new ArgumentException("La cuenta de la sesión es obligatoria.", nameof(accountId));
        }

        if (string.IsNullOrWhiteSpace(tokenDigest))
        {
            throw new ArgumentException("El resumen del token es obligatorio.", nameof(tokenDigest));
        }

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentException("La expiración debe ser posterior a la creación.", nameof(expiresAtUtc));
        }

        return new RefreshSession(accountId, tokenDigest, createdAtUtc, expiresAtUtc, null);
    }

    public bool IsActiveAt(DateTimeOffset instantUtc) =>
        RevokedAtUtc is null && ExpiresAtUtc > instantUtc;

    public RefreshSession RevokeAt(DateTimeOffset instantUtc) => this with { RevokedAtUtc = instantUtc };
}