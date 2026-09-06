namespace ConferenceRoomBookingApi.Application.DTOs.Analytics;

/// <summary>
/// Metric data transfer object representing the utilization rate of a conference room.
/// </summary>
public class RoomUtilizationDto
{
    /// <summary>
    /// The unique identifier of the room.
    /// </summary>
    /// <example>1</example>
    public int RoomId { get; set; }

    /// <summary>
    /// The display name of the room.
    /// </summary>
    /// <example>Room A</example>
    public string RoomName { get; set; } = string.Empty;

    /// <summary>
    /// Total hours the room was booked in the evaluation period.
    /// </summary>
    /// <example>68</example>
    public int TotalBookedHours { get; set; }

    /// <summary>
    /// Total available operational hours in the evaluation period (e.g., 17 hrs/day: 06:00 to 23:00).
    /// </summary>
    /// <example>510</example>
    public int TotalAvailableHours { get; set; }

    /// <summary>
    /// Room utilization percentage (TotalBookedHours / TotalAvailableHours * 100).
    /// </summary>
    /// <example>13.33</example>
    public decimal UtilizationPercentage { get; set; }
}
