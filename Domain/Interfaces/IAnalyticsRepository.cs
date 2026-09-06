using ConferenceRoomBookingApi.Application.DTOs.Analytics;

namespace ConferenceRoomBookingApi.Domain.Interfaces;

/// <summary>
/// Repository abstraction for calculating business and operational metrics.
/// </summary>
public interface IAnalyticsRepository
{
    /// <summary>
    /// Computes total revenue and booking counts per conference room.
    /// </summary>
    Task<IEnumerable<RoomRevenueDto>> GetRoomRevenueMetricsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Computes room utilization rates based on booked hours versus available operating hours.
    /// </summary>
    Task<IEnumerable<RoomUtilizationDto>> GetRoomUtilizationMetricsAsync(DateTime? startDate = null, DateTime? endDate = null, int dailyOperatingHours = 17);

    /// <summary>
    /// Retrieves the most requested add-on services ranked by reservation count.
    /// </summary>
    Task<IEnumerable<PopularServiceDto>> GetMostRequestedServicesAsync(int top = 10);

    /// <summary>
    /// Retrieves an aggregated dashboard summary containing revenue, utilization, and popular services.
    /// </summary>
    Task<AnalyticsSummaryDto> GetSummaryAsync(DateTime? startDate = null, DateTime? endDate = null, int dailyOperatingHours = 17);
}
