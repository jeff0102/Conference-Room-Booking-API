using ConferenceRoomBookingApi.Application.DTOs.Service;
using ConferenceRoomBookingApi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        // Ready to be integrated with Dapper repository
        return Ok(Array.Empty<ServiceDto>());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id)
    {
        return NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateServiceDto dto)
    {
        return CreatedAtAction(nameof(GetById), new { id = 1 }, dto);
    }
}
