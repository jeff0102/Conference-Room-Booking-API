namespace ConferenceRoomBookingApi.Application.Services;

using ConferenceRoomBookingApi.Domain.Entities;

public class BookingPriceCalculator : IBookingPriceCalculator
{
    public decimal CalculateTotalPrice(decimal basePricePerHour, int durationHours, IEnumerable<Service> selectedServices)
    {
        if (durationHours <= 0)
        {
            throw new ArgumentException("Duration must be greater than zero.", nameof(durationHours));
        }

        var servicesTotal = selectedServices?.Sum(s => s.Price) ?? 0m;
        var roomTotal = basePricePerHour * durationHours;

        return roomTotal + servicesTotal;
    }
}
