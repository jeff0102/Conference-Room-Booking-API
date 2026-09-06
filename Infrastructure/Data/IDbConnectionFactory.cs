namespace ConferenceRoomBookingApi.Infrastructure.Data;

using System.Data;

/// <summary>
/// Abstraction for creating database connections, enabling clean integration with Dapper.
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
