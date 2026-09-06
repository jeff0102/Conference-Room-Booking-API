namespace ConferenceRoomBookingApi.Application.Services;

using ConferenceRoomBookingApi.Domain.Entities;

public interface IBookingPriceCalculator
{
    /// <summary>
    /// Calculates the room rental price based on hourly rates, discounts, and surcharges.
    /// Supports prorated calculation across multiple intervals.
    /// </summary>
    decimal CalculateRoomPrice(decimal basePricePerHour, DateTime bookingDate, int durationHours);

    /// <summary>
    /// Calculates the total booking price including room rental and selected additional services.
    /// </summary>
    decimal CalculateTotalPrice(decimal basePricePerHour, DateTime bookingDate, int durationHours, IEnumerable<Service>? selectedServices = null);

    /// <summary>
    /// Returns the rate multiplier for a given time of day.
    /// Morning (06:00 - 09:00): 0.90 (-10%)
    /// Standard (09:00 - 12:00, 14:00 - 18:00): 1.00 (Base rate)
    /// Peak (12:00 - 14:00): 1.15 (+15%)
    /// Evening/Night (18:00 - 23:00): 0.80 (-20%)
    /// </summary>
    decimal GetRateMultiplier(TimeSpan timeOfDay);
}
