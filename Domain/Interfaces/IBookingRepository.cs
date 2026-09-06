namespace ConferenceRoomBookingApi.Domain.Interfaces;

using ConferenceRoomBookingApi.Domain.Entities;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(int id);
    Task<int> CreateAsync(Booking booking);
    Task<bool> UpdateAsync(Booking booking);
    Task<bool> DeleteAsync(int id);
    Task<bool> HasConflictAsync(int roomId, DateTime bookingDate, int durationHours, int? excludeBookingId = null);
}
