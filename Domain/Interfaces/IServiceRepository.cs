namespace ConferenceRoomBookingApi.Domain.Interfaces;

using ConferenceRoomBookingApi.Domain.Entities;

public interface IServiceRepository
{
    Task<IEnumerable<Service>> GetAllAsync();
    Task<Service?> GetByIdAsync(int id);
    Task<IEnumerable<Service>> GetByIdsAsync(IEnumerable<int> ids);
    Task<int> CreateAsync(Service service);
    Task<bool> UpdateAsync(Service service);
    Task<bool> DeleteAsync(int id);
}
