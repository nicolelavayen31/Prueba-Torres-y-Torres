using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AuthService.Infrastructure.Persistence;

public sealed class SqlAccountStore(SqlConnectionFactory connectionFactory) : IAccountDirectory, IAccountRepository
{
    public async Task<Account?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Email, DisplayName, PasswordHash, CreatedAt
            FROM dbo.Users
            WHERE Email = @Email;
            """;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = email;

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadAccount(reader) : null;
    }

    public async Task<Account?> FindByIdAsync(int accountId, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Email, DisplayName, PasswordHash, CreatedAt
            FROM dbo.Users
            WHERE Id = @Id;
            """;
        command.Parameters.Add("@Id", SqlDbType.Int).Value = accountId;

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadAccount(reader) : null;
    }

    public async Task<Account?> TryAddAsync(Account account, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO dbo.Users (Email, DisplayName, PasswordHash, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Email, @DisplayName, @PasswordHash, @CreatedAt);
            """;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = account.Email.Value;
        command.Parameters.Add("@DisplayName", SqlDbType.NVarChar, 80).Value = account.DisplayName;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = account.PasswordHash;
        command.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = account.CreatedAtUtc.UtcDateTime;

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is int accountId ? account.WithId(accountId) : null;
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            return null;
        }
    }

    private static Account ReadAccount(SqlDataReader reader)
    {
        var createdAt = DateTime.SpecifyKind(reader.GetDateTime(4), DateTimeKind.Utc);
        return Account.Register(
                EmailAddress.Create(reader.GetString(1)),
                reader.GetString(2),
                reader.GetString(3),
                new DateTimeOffset(createdAt))
            .WithId(reader.GetInt32(0));
    }
}