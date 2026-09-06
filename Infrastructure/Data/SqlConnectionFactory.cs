namespace ConferenceRoomBookingApi.Infrastructure.Data;

using System.Data;
using Microsoft.Data.SqlClient;

/// <summary>
/// SQL Server implementation of IDbConnectionFactory.
/// </summary>
public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
