using System.Data;
using ConferenceRoomBookingApi.Domain.Entities;
using ConferenceRoomBookingApi.Domain.Interfaces;
using ConferenceRoomBookingApi.Infrastructure.Data;
using Dapper;

namespace ConferenceRoomBookingApi.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoomRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<IEnumerable<Room>> GetAllAsync()
    {
        const string sql = @"
            SELECT Id, Name, Capacity, BasePricePerHour
            FROM Rooms
            ORDER BY Name ASC;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Room>(sql);
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Name, Capacity, BasePricePerHour
            FROM Rooms
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Room>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Room room)
    {
        if (room == null) throw new ArgumentNullException(nameof(room));

        const string sql = @"
            INSERT INTO Rooms (Name, Capacity, BasePricePerHour)
            VALUES (@Name, @Capacity, @BasePricePerHour);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = _connectionFactory.CreateConnection();
        var newId = await connection.ExecuteScalarAsync<int>(sql, new
        {
            room.Name,
            room.Capacity,
            room.BasePricePerHour
        });

        room.Id = newId;
        return newId;
    }

    public async Task<bool> UpdateAsync(Room room)
    {
        if (room == null) throw new ArgumentNullException(nameof(room));

        const string sql = @"
            UPDATE Rooms
            SET Name = @Name,
                Capacity = @Capacity,
                BasePricePerHour = @BasePricePerHour
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, new
        {
            room.Id,
            room.Name,
            room.Capacity,
            room.BasePricePerHour
        });

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
            DELETE FROM Rooms
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }
}
