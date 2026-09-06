using System.Data;
using System.Data.Common;
using ConferenceRoomBookingApi.Domain.Entities;
using ConferenceRoomBookingApi.Domain.Interfaces;
using ConferenceRoomBookingApi.Infrastructure.Data;
using Dapper;

namespace ConferenceRoomBookingApi.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BookingRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<IEnumerable<Booking>> GetAllAsync()
    {
        const string sql = @"
            SELECT 
                b.Id, b.RoomId, b.BookingDate, b.DurationHours, b.TotalPrice,
                r.Id, r.Name, r.Capacity, r.BasePricePerHour,
                s.Id, s.Name, s.Price
            FROM Bookings b
            INNER JOIN Rooms r ON b.RoomId = r.Id
            LEFT JOIN BookingServices bs ON b.Id = bs.BookingId
            LEFT JOIN Services s ON bs.ServiceId = s.Id
            ORDER BY b.BookingDate DESC;";

        using var connection = _connectionFactory.CreateConnection();
        var bookingDictionary = new Dictionary<int, Booking>();

        await connection.QueryAsync<Booking, Room, Service, Booking>(
            sql,
            (booking, room, service) =>
            {
                if (!bookingDictionary.TryGetValue(booking.Id, out var currentBooking))
                {
                    currentBooking = booking;
                    currentBooking.Room = room;
                    currentBooking.SelectedServices = new List<Service>();
                    bookingDictionary.Add(currentBooking.Id, currentBooking);
                }

                if (service != null && service.Id > 0 && !currentBooking.SelectedServices.Any(s => s.Id == service.Id))
                {
                    currentBooking.SelectedServices.Add(service);
                }

                return currentBooking;
            },
            splitOn: "Id,Id");

        return bookingDictionary.Values;
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT 
                b.Id, b.RoomId, b.BookingDate, b.DurationHours, b.TotalPrice,
                r.Id, r.Name, r.Capacity, r.BasePricePerHour,
                s.Id, s.Name, s.Price
            FROM Bookings b
            INNER JOIN Rooms r ON b.RoomId = r.Id
            LEFT JOIN BookingServices bs ON b.Id = bs.BookingId
            LEFT JOIN Services s ON bs.ServiceId = s.Id
            WHERE b.Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var bookingDictionary = new Dictionary<int, Booking>();

        await connection.QueryAsync<Booking, Room, Service, Booking>(
            sql,
            (booking, room, service) =>
            {
                if (!bookingDictionary.TryGetValue(booking.Id, out var currentBooking))
                {
                    currentBooking = booking;
                    currentBooking.Room = room;
                    currentBooking.SelectedServices = new List<Service>();
                    bookingDictionary.Add(currentBooking.Id, currentBooking);
                }

                if (service != null && service.Id > 0 && !currentBooking.SelectedServices.Any(s => s.Id == service.Id))
                {
                    currentBooking.SelectedServices.Add(service);
                }

                return currentBooking;
            },
            new { Id = id },
            splitOn: "Id,Id");

        return bookingDictionary.Values.FirstOrDefault();
    }

    public async Task<int> CreateAsync(Booking booking)
    {
        if (booking == null) throw new ArgumentNullException(nameof(booking));

        const string insertBookingSql = @"
            INSERT INTO Bookings (RoomId, BookingDate, DurationHours, TotalPrice)
            VALUES (@RoomId, @BookingDate, @DurationHours, @TotalPrice);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = _connectionFactory.CreateConnection();
        if (connection is DbConnection dbConnection)
        {
            await dbConnection.OpenAsync();
        }
        else
        {
            connection.Open();
        }

        using var transaction = connection.BeginTransaction();
        try
        {
            var bookingId = await connection.ExecuteScalarAsync<int>(
                insertBookingSql,
                new
                {
                    booking.RoomId,
                    booking.BookingDate,
                    booking.DurationHours,
                    booking.TotalPrice
                },
                transaction: transaction);

            if (booking.SelectedServices != null && booking.SelectedServices.Any())
            {
                const string insertBookingServiceSql = @"
                    INSERT INTO BookingServices (BookingId, ServiceId)
                    VALUES (@BookingId, @ServiceId);";

                var junctionItems = booking.SelectedServices.Select(s => new
                {
                    BookingId = bookingId,
                    ServiceId = s.Id
                });

                await connection.ExecuteAsync(insertBookingServiceSql, junctionItems, transaction: transaction);
            }

            transaction.Commit();
            booking.Id = bookingId;
            return bookingId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Booking booking)
    {
        if (booking == null) throw new ArgumentNullException(nameof(booking));

        const string updateBookingSql = @"
            UPDATE Bookings
            SET RoomId = @RoomId,
                BookingDate = @BookingDate,
                DurationHours = @DurationHours,
                TotalPrice = @TotalPrice
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        if (connection is DbConnection dbConnection)
        {
            await dbConnection.OpenAsync();
        }
        else
        {
            connection.Open();
        }

        using var transaction = connection.BeginTransaction();
        try
        {
            var affected = await connection.ExecuteAsync(
                updateBookingSql,
                new
                {
                    booking.Id,
                    booking.RoomId,
                    booking.BookingDate,
                    booking.DurationHours,
                    booking.TotalPrice
                },
                transaction: transaction);

            if (affected == 0)
            {
                transaction.Rollback();
                return false;
            }

            const string deleteServicesSql = @"DELETE FROM BookingServices WHERE BookingId = @BookingId;";
            await connection.ExecuteAsync(deleteServicesSql, new { BookingId = booking.Id }, transaction: transaction);

            if (booking.SelectedServices != null && booking.SelectedServices.Any())
            {
                const string insertBookingServiceSql = @"
                    INSERT INTO BookingServices (BookingId, ServiceId)
                    VALUES (@BookingId, @ServiceId);";

                var junctionItems = booking.SelectedServices.Select(s => new
                {
                    BookingId = booking.Id,
                    ServiceId = s.Id
                });

                await connection.ExecuteAsync(insertBookingServiceSql, junctionItems, transaction: transaction);
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
            DELETE FROM BookingServices WHERE BookingId = @Id;
            DELETE FROM Bookings WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<bool> HasConflictAsync(int roomId, DateTime bookingDate, int durationHours, int? excludeBookingId = null)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM Bookings
            WHERE RoomId = @RoomId
              AND (@ExcludeBookingId IS NULL OR Id <> @ExcludeBookingId)
              AND BookingDate < DATEADD(hour, @DurationHours, @BookingDate)
              AND DATEADD(hour, DurationHours, BookingDate) > @BookingDate;";

        using var connection = _connectionFactory.CreateConnection();
        var conflictCount = await connection.ExecuteScalarAsync<int>(sql, new
        {
            RoomId = roomId,
            BookingDate = bookingDate,
            DurationHours = durationHours,
            ExcludeBookingId = excludeBookingId
        });

        return conflictCount > 0;
    }
}
