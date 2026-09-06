namespace ConferenceRoomBookingApi.Domain.Entities;

/// <summary>
/// Represents the junction table entity linking Bookings and Services for relational database storage.
/// </summary>
public class BookingServiceItem
{
    public int BookingId { get; set; }
    public int ServiceId { get; set; }

    public BookingServiceItem() { }

    public BookingServiceItem(int bookingId, int serviceId)
    {
        BookingId = bookingId;
        ServiceId = serviceId;
    }
}
