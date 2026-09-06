using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomBookingApi.Application.DTOs.Room;

public class UpdateRoomDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int Capacity { get; set; }

    [Range(0.01, 100000.0)]
    public decimal BasePricePerHour { get; set; }
}
