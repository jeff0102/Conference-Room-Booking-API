namespace ConferenceRoomBookingApi.Application.Services;

using ConferenceRoomBookingApi.Domain.Entities;

public interface IBookingPriceCalculator
{
    decimal CalculateTotalPrice(decimal basePricePerHour, int durationHours, IEnumerable<Service> selectedServices);
}
