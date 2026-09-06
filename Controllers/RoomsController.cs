using ConferenceRoomBookingApi.Application.DTOs.Room;
using ConferenceRoomBookingApi.Domain.Entities;
using ConferenceRoomBookingApi.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBookingApi.Controllers;

/// <summary>
/// Manages conference rooms inventory, capacity, and base hourly pricing.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RoomsController : ControllerBase
{
    private readonly IRoomRepository _roomRepository;

    public RoomsController(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    /// <summary>
    /// Retrieves all available conference rooms.
    /// </summary>
    /// <response code="200">Returns the full collection of conference rooms.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoomDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _roomRepository.GetAllAsync();
        var dtos = rooms.Select(r => new RoomDto
        {
            Id = r.Id,
            Name = r.Name,
            Capacity = r.Capacity,
            BasePricePerHour = r.BasePricePerHour
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Retrieves a specific conference room by its unique identifier.
    /// </summary>
    /// <param name="id">The integer ID of the room.</param>
    /// <response code="200">Returns the room details.</response>
    /// <response code="404">If the room with the specified ID does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room == null)
        {
            return NotFound(new { message = $"Room with ID {id} not found." });
        }

        return Ok(new RoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Capacity = room.Capacity,
            BasePricePerHour = room.BasePricePerHour
        });
    }

    /// <summary>
    /// Creates a new conference room.
    /// </summary>
    /// <param name="dto">The room creation payload containing name, capacity, and hourly rate.</param>
    /// <response code="201">Returns the newly created room with generated ID.</response>
    /// <response code="400">If the input model fails validation rules.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var room = new Room
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            BasePricePerHour = dto.BasePricePerHour
        };

        var id = await _roomRepository.CreateAsync(room);
        var createdDto = new RoomDto
        {
            Id = id,
            Name = room.Name,
            Capacity = room.Capacity,
            BasePricePerHour = room.BasePricePerHour
        };

        return CreatedAtAction(nameof(GetById), new { id }, createdDto);
    }
}
