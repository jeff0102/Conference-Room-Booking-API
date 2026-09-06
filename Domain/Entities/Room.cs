namespace ConferenceRoomBookingApi.Domain.Entities;

/// <summary>
/// Represents a conference room available for booking.
/// </summary>
public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BasePricePerHour { get; set; }

    public Room() { }

    public Room(int id, string name, int capacity, decimal basePricePerHour)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
        BasePricePerHour = basePricePerHour;
    }
}
