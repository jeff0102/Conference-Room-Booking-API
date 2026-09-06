namespace ConferenceRoomBookingApi.Domain.Entities;

/// <summary>
/// Represents an additional service or equipment that can be booked with a room (e.g. Projector, Catering, Coffee Break).
/// </summary>
public class Service
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public Service() { }

    public Service(int id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}
