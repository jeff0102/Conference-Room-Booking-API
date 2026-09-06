using ConferenceRoomBookingApi.Application.Services;

namespace ConferenceRoomBookingApi.Domain.Entities;

/// <summary>
/// Represents a booking reservation for a conference room including selected services.
/// </summary>
public class Booking
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public DateTime BookingDate { get; set; }
    public int DurationHours { get; set; }
    public List<Service> SelectedServices { get; set; } = new();
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Optional navigation property to the booked Room.
    /// </summary>
    public Room? Room { get; set; }

    public Booking() { }

    public Booking(
        int id,
        int roomId,
        DateTime bookingDate,
        int durationHours,
        List<Service>? selectedServices = null,
        decimal totalPrice = 0)
    {
        Id = id;
        RoomId = roomId;
        BookingDate = bookingDate;
        DurationHours = durationHours;
        SelectedServices = selectedServices ?? new List<Service>();
        TotalPrice = totalPrice;
    }

    /// <summary>
    /// Calculates the total price using the business rule price calculator, or fallback base rate.
    /// </summary>
    public decimal CalculateTotalPrice(decimal roomBasePricePerHour, IBookingPriceCalculator? calculator = null)
    {
        if (calculator != null)
        {
            TotalPrice = calculator.CalculateTotalPrice(roomBasePricePerHour, BookingDate, DurationHours, SelectedServices);
            return TotalPrice;
        }

        var servicesTotal = SelectedServices.Sum(s => s.Price);
        var roomCost = roomBasePricePerHour * DurationHours;
        TotalPrice = roomCost + servicesTotal;
        return TotalPrice;
    }
}
