using System.Data;
using ConferenceRoomBookingApi.Application.DTOs.Analytics;
using ConferenceRoomBookingApi.Domain.Interfaces;
using ConferenceRoomBookingApi.Infrastructure.Data;
using Dapper;

namespace ConferenceRoomBookingApi.Infrastructure.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AnalyticsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<RoomRevenueDto>> GetRoomRevenueMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        const string sql = @"
            SELECT 
                r.Id AS RoomId,
                r.Name AS RoomName,
                COUNT(b.Id) AS TotalBookings,
                COALESCE(SUM(b.TotalPrice), 0) AS TotalRevenue
            FROM Rooms r
            LEFT JOIN Bookings b ON r.Id = b.RoomId
                AND (@StartDate IS NULL OR b.BookingDate >= @StartDate)
                AND (@EndDate IS NULL OR b.BookingDate <= @EndDate)
            GROUP BY r.Id, r.Name
            ORDER BY TotalRevenue DESC, r.Name ASC;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<RoomRevenueDto>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<IEnumerable<RoomUtilizationDto>> GetRoomUtilizationMetricsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int dailyOperatingHours = 17)
    {
        const string sql = @"
            SELECT 
                r.Id AS RoomId,
                r.Name AS RoomName,
                COALESCE(SUM(b.DurationHours), 0) AS TotalBookedHours
            FROM Rooms r
            LEFT JOIN Bookings b ON r.Id = b.RoomId
                AND (@StartDate IS NULL OR b.BookingDate >= @StartDate)
                AND (@EndDate IS NULL OR b.BookingDate <= @EndDate)
            GROUP BY r.Id, r.Name
            ORDER BY r.Name ASC;";

        using var connection = _connectionFactory.CreateConnection();
        var rawMetrics = (await connection.QueryAsync<(int RoomId, string RoomName, int TotalBookedHours)>(
            sql, new { StartDate = startDate, EndDate = endDate })).ToList();

        // Compute evaluated period days (defaulting to a 30-day window if unbounded)
        int evaluatedDays;
        if (startDate.HasValue && endDate.HasValue)
        {
            evaluatedDays = Math.Max(1, (endDate.Value.Date - startDate.Value.Date).Days + 1);
        }
        else if (startDate.HasValue)
        {
            evaluatedDays = Math.Max(1, (DateTime.UtcNow.Date - startDate.Value.Date).Days + 1);
        }
        else
        {
            evaluatedDays = 30; // Default evaluation window
        }

        var totalAvailableHours = evaluatedDays * Math.Max(1, dailyOperatingHours);

        return rawMetrics.Select(m => new RoomUtilizationDto
        {
            RoomId = m.RoomId,
            RoomName = m.RoomName,
            TotalBookedHours = m.TotalBookedHours,
            TotalAvailableHours = totalAvailableHours,
            UtilizationPercentage = totalAvailableHours > 0
                ? Math.Round(((decimal)m.TotalBookedHours / totalAvailableHours) * 100m, 2)
                : 0m
        });
    }

    public async Task<IEnumerable<PopularServiceDto>> GetMostRequestedServicesAsync(int top = 10)
    {
        var safeTop = Math.Clamp(top, 1, 100);
        const string sql = @"
            SELECT TOP (@Top)
                s.Id AS ServiceId,
                s.Name AS ServiceName,
                s.Price AS UnitPrice,
                COUNT(bs.BookingId) AS RequestCount,
                COALESCE(COUNT(bs.BookingId) * s.Price, 0) AS TotalRevenue
            FROM Services s
            LEFT JOIN BookingServices bs ON s.Id = bs.ServiceId
            GROUP BY s.Id, s.Name, s.Price
            ORDER BY RequestCount DESC, TotalRevenue DESC, s.Name ASC;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<PopularServiceDto>(sql, new { Top = safeTop });
    }

    public async Task<AnalyticsSummaryDto> GetSummaryAsync(DateTime? startDate = null, DateTime? endDate = null, int dailyOperatingHours = 17)
    {
        var revenues = (await GetRoomRevenueMetricsAsync(startDate, endDate)).ToList();
        var utilizations = (await GetRoomUtilizationMetricsAsync(startDate, endDate, dailyOperatingHours)).ToList();
        var services = (await GetMostRequestedServicesAsync(10)).ToList();

        return new AnalyticsSummaryDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalOverallRevenue = revenues.Sum(r => r.TotalRevenue),
            TotalBookings = revenues.Sum(r => r.TotalBookings),
            RoomRevenues = revenues,
            RoomUtilizations = utilizations,
            PopularServices = services
        };
    }
}
