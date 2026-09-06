namespace ConferenceRoomBookingApi.Application.DTOs.Analytics;

/// <summary>
/// Metric data transfer object representing add-on service request demand.
/// </summary>
public class PopularServiceDto
{
    /// <summary>
    /// The unique identifier of the service.
    /// </summary>
    /// <example>1</example>
    public int ServiceId { get; set; }

    /// <summary>
    /// The display name of the add-on service.
    /// </summary>
    /// <example>Projector</example>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Unit price of the add-on service (in UAH).
    /// </summary>
    /// <example>500.00</example>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total number of times this service was selected in reservations.
    /// </summary>
    /// <example>24</example>
    public int RequestCount { get; set; }

    /// <summary>
    /// Total revenue generated exclusively by this add-on service (in UAH).
    /// </summary>
    /// <example>12000.00</example>
    public decimal TotalRevenue { get; set; }
}
