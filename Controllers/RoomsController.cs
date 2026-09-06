using ConferenceRoomBookingApi.Application.DTOs.Room;
using ConferenceRoomBookingApi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoomDto>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        // Ready to be integrated with Dapper repository
        return Ok(Array.Empty<RoomDto>());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id)
    {
        return NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateRoomDto dto)
    {
        return CreatedAtAction(nameof(GetById), new { id = 1 }, dto);
    }
}
