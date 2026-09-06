using ConferenceRoomBookingApi.Application.DTOs.Booking;
using ConferenceRoomBookingApi.Application.DTOs.Room;
using ConferenceRoomBookingApi.Application.DTOs.Service;
using ConferenceRoomBookingApi.Application.Services;
using ConferenceRoomBookingApi.Domain.Entities;
using ConferenceRoomBookingApi.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBookingApi.Controllers;

/// <summary>
/// Handles conference room reservations, dynamic multi-rate pricing calculations, conflict checks, and bookings query operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IBookingPriceCalculator _priceCalculator;

    public BookingsController(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IServiceRepository serviceRepository,
        IBookingPriceCalculator priceCalculator)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _serviceRepository = serviceRepository;
        _priceCalculator = priceCalculator;
    }

    /// <summary>
    /// Retrieves all reservations including room details and selected add-on services.
    /// </summary>
    /// <response code="200">Returns the full collection of conference room bookings.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var bookings = await _bookingRepository.GetAllAsync();
        var dtos = bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            RoomId = b.RoomId,
            Room = b.Room != null ? new RoomDto
            {
                Id = b.Room.Id,
                Name = b.Room.Name,
                Capacity = b.Room.Capacity,
                BasePricePerHour = b.Room.BasePricePerHour
            } : null,
            BookingDate = b.BookingDate,
            DurationHours = b.DurationHours,
            TotalPrice = b.TotalPrice,
            SelectedServices = b.SelectedServices.Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price
            }).ToList()
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Retrieves a specific reservation by its unique identifier.
    /// </summary>
    /// <param name="id">The integer ID of the booking.</param>
    /// <response code="200">Returns the booking reservation details.</response>
    /// <response code="404">If the booking with the specified ID was not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking == null)
        {
            return NotFound(new { message = $"Booking with ID {id} not found." });
        }

        return Ok(new BookingDto
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            Room = booking.Room != null ? new RoomDto
            {
                Id = booking.Room.Id,
                Name = booking.Room.Name,
                Capacity = booking.Room.Capacity,
                BasePricePerHour = booking.Room.BasePricePerHour
            } : null,
            BookingDate = booking.BookingDate,
            DurationHours = booking.DurationHours,
            TotalPrice = booking.TotalPrice,
            SelectedServices = booking.SelectedServices.Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price
            }).ToList()
        });
    }

    /// <summary>
    /// Creates a new conference room booking after verifying room availability, conflict overlaps, and applying dynamic time-of-day pricing.
    /// </summary>
    /// <remarks>
    /// Pricing calculation rules applied:
    /// - Morning hours (06:00 - 09:00): 10% discount
    /// - Standard hours (09:00 - 12:00, 14:00 - 18:00): base rate
    /// - Peak hours (12:00 - 14:00): 15% surcharge
    /// - Evening hours (18:00 - 23:00): 20% discount
    /// - Multi-interval bookings are dynamically prorated hour-by-hour (or per minute).
    /// - Selected add-on services are added to the calculated room rental total.
    /// </remarks>
    /// <param name="dto">The reservation creation request.</param>
    /// <response code="201">Returns the created booking with calculated total price and ID.</response>
    /// <response code="400">If the room does not exist or input validation fails.</response>
    /// <response code="409">If the conference room is already reserved for the requested time slot.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var room = await _roomRepository.GetByIdAsync(dto.RoomId);
        if (room == null)
        {
            return BadRequest(new { message = $"Room with ID {dto.RoomId} does not exist." });
        }

        var hasConflict = await _bookingRepository.HasConflictAsync(dto.RoomId, dto.BookingDate, dto.DurationHours);
        if (hasConflict)
        {
            return Conflict(new { message = "The room is already booked for the selected time slot." });
        }

        var selectedServices = (await _serviceRepository.GetByIdsAsync(dto.ServiceIds)).ToList();
        var totalPrice = _priceCalculator.CalculateTotalPrice(room.BasePricePerHour, dto.BookingDate, dto.DurationHours, selectedServices);

        var booking = new Booking
        {
            RoomId = dto.RoomId,
            BookingDate = dto.BookingDate,
            DurationHours = dto.DurationHours,
            SelectedServices = selectedServices,
            TotalPrice = totalPrice
        };

        var id = await _bookingRepository.CreateAsync(booking);

        var result = new BookingDto
        {
            Id = id,
            RoomId = booking.RoomId,
            Room = new RoomDto
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity,
                BasePricePerHour = room.BasePricePerHour
            },
            BookingDate = booking.BookingDate,
            DurationHours = booking.DurationHours,
            TotalPrice = totalPrice,
            SelectedServices = selectedServices.Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price
            }).ToList()
        };

        return CreatedAtAction(nameof(GetById), new { id }, result);
    }
}
