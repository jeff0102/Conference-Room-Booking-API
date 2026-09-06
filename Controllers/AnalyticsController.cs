using ConferenceRoomBookingApi.Application.DTOs.Analytics;
using ConferenceRoomBookingApi.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBookingApi.Controllers;

/// <summary>
/// Provides operational reporting and business intelligence metrics for conference room usage and revenue.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public AnalyticsController(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    /// <summary>
    /// Retrieves an aggregated dashboard summary including overall revenue, booking counts, utilization rates, and top add-on services.
    /// </summary>
    /// <param name="startDate">Optional lower date boundary (ISO-8601 format: YYYY-MM-DD).</param>
    /// <param name="endDate">Optional upper date boundary (ISO-8601 format: YYYY-MM-DD).</param>
    /// <param name="dailyOperatingHours">Daily operational hours (defaults to 17 hours: 06:00 to 23:00).</param>
    /// <response code="200">Returns the aggregated analytics summary.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(AnalyticsSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int dailyOperatingHours = 17)
    {
        var summary = await _analyticsRepository.GetSummaryAsync(startDate, endDate, dailyOperatingHours);
        return Ok(summary);
    }

    /// <summary>
    /// Computes total revenue and total booking counts grouped per conference room.
    /// </summary>
    /// <param name="startDate">Optional filter start date (ISO-8601 format: YYYY-MM-DD).</param>
    /// <param name="endDate">Optional filter end date (ISO-8601 format: YYYY-MM-DD).</param>
    /// <response code="200">Returns room revenue and booking metrics.</response>
    [HttpGet("room-revenue")]
    [ProducesResponseType(typeof(IEnumerable<RoomRevenueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomRevenue(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var metrics = await _analyticsRepository.GetRoomRevenueMetricsAsync(startDate, endDate);
        return Ok(metrics);
    }

    /// <summary>
    /// Calculates room utilization rate as a percentage of booked hours versus available operating hours.
    /// </summary>
    /// <param name="startDate">Optional filter start date (ISO-8601 format: YYYY-MM-DD).</param>
    /// <param name="endDate">Optional filter end date (ISO-8601 format: YYYY-MM-DD).</param>
    /// <param name="dailyOperatingHours">Operating hours per day (default is 17 hours: 06:00 - 23:00).</param>
    /// <response code="200">Returns utilization statistics for each room.</response>
    [HttpGet("room-utilization")]
    [ProducesResponseType(typeof(IEnumerable<RoomUtilizationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomUtilization(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int dailyOperatingHours = 17)
    {
        var metrics = await _analyticsRepository.GetRoomUtilizationMetricsAsync(startDate, endDate, dailyOperatingHours);
        return Ok(metrics);
    }

    /// <summary>
    /// Identifies the most requested add-on services ranked by reservation count and revenue contribution.
    /// </summary>
    /// <param name="top">Maximum number of top services to return (between 1 and 100, default is 10).</param>
    /// <response code="200">Returns the ranking of requested add-on services.</response>
    [HttpGet("popular-services")]
    [ProducesResponseType(typeof(IEnumerable<PopularServiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopularServices([FromQuery] int top = 10)
    {
        var metrics = await _analyticsRepository.GetMostRequestedServicesAsync(top);
        return Ok(metrics);
    }
}
