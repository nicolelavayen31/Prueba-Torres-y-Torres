using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Persistence;

public sealed class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AuthDb")
            ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:AuthDb.");
    }

    public SqlConnection CreateConnection() => new(_connectionString);
}