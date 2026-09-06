using ConferenceRoomBookingApi.Application.DTOs.Booking;
using ConferenceRoomBookingApi.Application.DTOs.Room;
using ConferenceRoomBookingApi.Application.DTOs.Service;
using ConferenceRoomBookingApi.Application.Services;
using ConferenceRoomBookingApi.Domain.Entities;
using ConferenceRoomBookingApi.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpPost]
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
