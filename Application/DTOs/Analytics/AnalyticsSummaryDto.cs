namespace ConferenceRoomBookingApi.Application.DTOs.Analytics;

/// <summary>
/// Aggregated analytics dashboard response containing high-level financial and operational KPIs.
/// </summary>
public class AnalyticsSummaryDto
{
    /// <summary>
    /// The start timestamp of the evaluated reporting window.
    /// </summary>
    /// <example>2026-09-01T00:00:00</example>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// The end timestamp of the evaluated reporting window.
    /// </summary>
    /// <example>2026-09-30T23:59:59</example>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Total overall revenue across all rooms and add-on services (in UAH).
    /// </summary>
    /// <example>115400.00</example>
    public decimal TotalOverallRevenue { get; set; }

    /// <summary>
    /// Total count of bookings placed in this reporting period.
    /// </summary>
    /// <example>42</example>
    public int TotalBookings { get; set; }

    /// <summary>
    /// Breakdown of revenue and booking counts per room.
    /// </summary>
    public List<RoomRevenueDto> RoomRevenues { get; set; } = new();

    /// <summary>
    /// Breakdown of utilization rates across conference rooms.
    /// </summary>
    public List<RoomUtilizationDto> RoomUtilizations { get; set; } = new();

    /// <summary>
    /// Breakdown of the most demanded add-on services.
    /// </summary>
    public List<PopularServiceDto> PopularServices { get; set; } = new();
}
