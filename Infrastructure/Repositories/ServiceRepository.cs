using System.Data;
using ConferenceRoomBookingApi.Domain.Entities;
using ConferenceRoomBookingApi.Domain.Interfaces;
using ConferenceRoomBookingApi.Infrastructure.Data;
using Dapper;

namespace ConferenceRoomBookingApi.Infrastructure.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ServiceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<IEnumerable<Service>> GetAllAsync()
    {
        const string sql = @"
            SELECT Id, Name, Price
            FROM Services
            ORDER BY Name ASC;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Service>(sql);
    }

    public async Task<Service?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Name, Price
            FROM Services
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Service>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Service>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids?.Distinct().ToList();
        if (idList == null || idList.Count == 0)
        {
            return Enumerable.Empty<Service>();
        }

        const string sql = @"
            SELECT Id, Name, Price
            FROM Services
            WHERE Id IN @Ids
            ORDER BY Name ASC;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Service>(sql, new { Ids = idList });
    }

    public async Task<int> CreateAsync(Service service)
    {
        if (service == null) throw new ArgumentNullException(nameof(service));

        const string sql = @"
            INSERT INTO Services (Name, Price)
            VALUES (@Name, @Price);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = _connectionFactory.CreateConnection();
        var newId = await connection.ExecuteScalarAsync<int>(sql, new
        {
            service.Name,
            service.Price
        });

        service.Id = newId;
        return newId;
    }

    public async Task<bool> UpdateAsync(Service service)
    {
        if (service == null) throw new ArgumentNullException(nameof(service));

        const string sql = @"
            UPDATE Services
            SET Name = @Name,
                Price = @Price
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, new
        {
            service.Id,
            service.Name,
            service.Price
        });

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
            DELETE FROM Services
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }
}
