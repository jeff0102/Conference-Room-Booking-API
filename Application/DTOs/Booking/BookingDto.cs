using ConferenceRoomBookingApi.Application.DTOs.Room;
using ConferenceRoomBookingApi.Application.DTOs.Service;

namespace ConferenceRoomBookingApi.Application.DTOs.Booking;

public class BookingDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public RoomDto? Room { get; set; }
    public DateTime BookingDate { get; set; }
    public int DurationHours { get; set; }
    public List<ServiceDto> SelectedServices { get; set; } = new();
    public decimal TotalPrice { get; set; }
}
