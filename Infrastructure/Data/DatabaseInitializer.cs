using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace ConferenceRoomBookingApi.Infrastructure.Data;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            const string createTablesSql = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rooms')
                BEGIN
                    CREATE TABLE Rooms (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        Capacity INT NOT NULL,
                        BasePricePerHour DECIMAL(18,2) NOT NULL
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Services')
                BEGIN
                    CREATE TABLE Services (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        Price DECIMAL(18,2) NOT NULL
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bookings')
                BEGIN
                    CREATE TABLE Bookings (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        RoomId INT NOT NULL,
                        BookingDate DATETIME2 NOT NULL,
                        DurationHours INT NOT NULL,
                        TotalPrice DECIMAL(18,2) NOT NULL,
                        CONSTRAINT FK_Bookings_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE
                    );
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BookingServices')
                BEGIN
                    CREATE TABLE BookingServices (
                        BookingId INT NOT NULL,
                        ServiceId INT NOT NULL,
                        PRIMARY KEY (BookingId, ServiceId),
                        CONSTRAINT FK_BookingServices_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings(Id) ON DELETE CASCADE,
                        CONSTRAINT FK_BookingServices_Services FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
                    );
                END;";

            await connection.ExecuteAsync(createTablesSql);
            _logger.LogInformation("Database tables verified/created successfully.");

            // Seed Initial Rooms using parameterized queries
            var initialRooms = new[]
            {
                new { Name = "Room A", Capacity = 50, BasePricePerHour = 2000.00m },
                new { Name = "Room B", Capacity = 100, BasePricePerHour = 3500.00m },
                new { Name = "Room C", Capacity = 30, BasePricePerHour = 1500.00m }
            };

            const string insertRoomSql = @"
                IF NOT EXISTS (SELECT 1 FROM Rooms WHERE Name = @Name)
                BEGIN
                    INSERT INTO Rooms (Name, Capacity, BasePricePerHour)
                    VALUES (@Name, @Capacity, @BasePricePerHour);
                END;";

            foreach (var room in initialRooms)
            {
                await connection.ExecuteAsync(insertRoomSql, room);
            }

            // Seed Initial Services using parameterized queries
            var initialServices = new[]
            {
                new { Name = "Projector", Price = 500.00m },
                new { Name = "Wi-Fi", Price = 300.00m },
                new { Name = "Sound", Price = 700.00m }
            };

            const string insertServiceSql = @"
                IF NOT EXISTS (SELECT 1 FROM Services WHERE Name = @Name)
                BEGIN
                    INSERT INTO Services (Name, Price)
                    VALUES (@Name, @Price);
                END;";

            foreach (var service in initialServices)
            {
                await connection.ExecuteAsync(insertServiceSql, service);
            }

            _logger.LogInformation("Initial database seed data verified/inserted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database initialization skipped or failed. Ensure database server is running and accessible.");
        }
    }
}
