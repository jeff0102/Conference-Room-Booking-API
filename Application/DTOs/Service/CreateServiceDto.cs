using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomBookingApi.Application.DTOs.Service;

public class CreateServiceDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(0.0, 100000.0)]
    public decimal Price { get; set; }
}
