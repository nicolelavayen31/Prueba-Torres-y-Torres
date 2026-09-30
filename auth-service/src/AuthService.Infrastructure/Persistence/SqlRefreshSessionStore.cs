using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AuthService.Infrastructure.Persistence;

public sealed class SqlRefreshSessionStore(SqlConnectionFactory connectionFactory) : IRefreshSessionStore
{
    public async Task<bool> TryAddAsync(RefreshSession session, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO dbo.RefreshSessions (UserId, TokenHash, CreatedAt, ExpiresAt)
            VALUES (@UserId, @TokenHash, @CreatedAt, @ExpiresAt);
            """;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = session.AccountId;
        command.Parameters.Add("@TokenHash", SqlDbType.Char, 64).Value = session.TokenDigest;
        command.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = session.CreatedAtUtc.UtcDateTime;
        command.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2).Value = session.ExpiresAtUtc.UtcDateTime;

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<RefreshSession?> TryConsumeAsync(
        string tokenDigest,
        int? expectedAccountId,
        DateTimeOffset instantUtc,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE dbo.RefreshSessions WITH (UPDLOCK, ROWLOCK)
            SET RevokedAt = @Now
            OUTPUT INSERTED.UserId, INSERTED.TokenHash, INSERTED.CreatedAt,
                   INSERTED.ExpiresAt, INSERTED.RevokedAt
            WHERE TokenHash = @TokenHash
              AND RevokedAt IS NULL
              AND ExpiresAt > @Now
              AND (@ExpectedUserId IS NULL OR UserId = @ExpectedUserId);
            """;
        command.Parameters.Add("@Now", SqlDbType.DateTime2).Value = instantUtc.UtcDateTime;
        command.Parameters.Add("@TokenHash", SqlDbType.Char, 64).Value = tokenDigest;
        command.Parameters.Add("@ExpectedUserId", SqlDbType.Int).Value =
            expectedAccountId is int userId ? userId : DBNull.Value;

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new RefreshSession(
            reader.GetInt32(0),
            reader.GetString(1).TrimEnd(),
            AsUtc(reader.GetDateTime(2)),
            AsUtc(reader.GetDateTime(3)),
            AsUtc(reader.GetDateTime(4)));
    }

    private static DateTimeOffset AsUtc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}