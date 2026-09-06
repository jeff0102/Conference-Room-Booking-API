using ConferenceRoomBookingApi.Application.DTOs.Room;
using ConferenceRoomBookingApi.Domain.Entities;
using ConferenceRoomBookingApi.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomRepository _roomRepository;

    public RoomsController(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

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

    [HttpPost]
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
