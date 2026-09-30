namespace AuthService.Application.Interfaces;

public interface IRefreshTokenIssuer
{
    RefreshTokenMaterial Create(DateTimeOffset issuedAtUtc);
    string ComputeDigest(string refreshToken);
}

public sealed record RefreshTokenMaterial(string Value, string Digest, DateTimeOffset ExpiresAtUtc);