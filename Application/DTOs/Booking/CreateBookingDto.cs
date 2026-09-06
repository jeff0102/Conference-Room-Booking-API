using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomBookingApi.Application.DTOs.Booking;

public class CreateBookingDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "A valid RoomId is required.")]
    public int RoomId { get; set; }

    [Required]
    public DateTime BookingDate { get; set; }

    [Required]
    [Range(1, 24, ErrorMessage = "Duration must be between 1 and 24 hours.")]
    public int DurationHours { get; set; }

    public List<int> ServiceIds { get; set; } = new();
}
