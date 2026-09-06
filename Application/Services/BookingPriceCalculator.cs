using ConferenceRoomBookingApi.Domain.Entities;

namespace ConferenceRoomBookingApi.Application.Services;

public class BookingPriceCalculator : IBookingPriceCalculator
{
    private static readonly TimeSpan MorningStart = new(6, 0, 0);
    private static readonly TimeSpan MorningEnd = new(9, 0, 0);

    private static readonly TimeSpan PeakStart = new(12, 0, 0);
    private static readonly TimeSpan PeakEnd = new(14, 0, 0);

    private static readonly TimeSpan EveningStart = new(18, 0, 0);
    private static readonly TimeSpan EveningEnd = new(23, 0, 0);

    private const decimal MorningDiscountMultiplier = 0.90m; // 10% discount
    private const decimal StandardRateMultiplier = 1.00m;    // Base hourly rate
    private const decimal PeakSurchargeMultiplier = 1.15m;   // 15% surcharge
    private const decimal EveningDiscountMultiplier = 0.80m; // 20% discount

    public decimal GetRateMultiplier(TimeSpan timeOfDay)
    {
        // Normalize time to within a 24-hour day in case of tick overflow
        var normalizedTicks = timeOfDay.Ticks % TimeSpan.FromDays(1).Ticks;
        if (normalizedTicks < 0)
        {
            normalizedTicks += TimeSpan.FromDays(1).Ticks;
        }

        var normalizedTime = TimeSpan.FromTicks(normalizedTicks);

        // Morning hours (06:00 - 09:00): 10% discount
        if (normalizedTime >= MorningStart && normalizedTime < MorningEnd)
        {
            return MorningDiscountMultiplier;
        }

        // Peak hours (12:00 - 14:00): 15% surcharge
        if (normalizedTime >= PeakStart && normalizedTime < PeakEnd)
        {
            return PeakSurchargeMultiplier;
        }

        // Evening/Night hours (18:00 - 23:00): 20% discount
        if (normalizedTime >= EveningStart && normalizedTime < EveningEnd)
        {
            return EveningDiscountMultiplier;
        }

        // Standard hours (09:00 - 12:00, 14:00 - 18:00, or other standard times)
        return StandardRateMultiplier;
    }

    public decimal CalculateRoomPrice(decimal basePricePerHour, DateTime bookingDate, int durationHours)
    {
        if (basePricePerHour < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(basePricePerHour), "Base price per hour cannot be negative.");
        }

        if (durationHours <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationHours), "Duration must be greater than zero.");
        }

        var totalMinutes = durationHours * 60;
        decimal weightedMinutesSum = 0m;

        for (var m = 0; m < totalMinutes; m++)
        {
            var minuteTime = bookingDate.AddMinutes(m);
            var multiplier = GetRateMultiplier(minuteTime.TimeOfDay);
            weightedMinutesSum += multiplier;
        }

        var roomPrice = (basePricePerHour * weightedMinutesSum) / 60m;
        return Math.Round(roomPrice, 2, MidpointRounding.AwayFromZero);
    }

    public decimal CalculateTotalPrice(decimal basePricePerHour, DateTime bookingDate, int durationHours, IEnumerable<Service>? selectedServices = null)
    {
        var roomPrice = CalculateRoomPrice(basePricePerHour, bookingDate, durationHours);
        var servicesTotal = selectedServices?.Sum(s => s.Price) ?? 0m;

        return Math.Round(roomPrice + servicesTotal, 2, MidpointRounding.AwayFromZero);
    }
}
