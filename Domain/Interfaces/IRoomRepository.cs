namespace ConferenceRoomBookingApi.Domain.Interfaces;

using ConferenceRoomBookingApi.Domain.Entities;

public interface IRoomRepository
{
    Task<IEnumerable<Room>> GetAllAsync();
    Task<Room?> GetByIdAsync(int id);
    Task<int> CreateAsync(Room room);
    Task<bool> UpdateAsync(Room room);
    Task<bool> DeleteAsync(int id);
}
