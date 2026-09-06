using ConferenceRoomBookingApi.Application.DTOs.Booking;
using ConferenceRoomBookingApi.Application.Services;
using ConferenceRoomBookingApi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingPriceCalculator _priceCalculator;

    public BookingsController(IBookingPriceCalculator priceCalculator)
    {
        _priceCalculator = priceCalculator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        // Ready to be integrated with Dapper repository
        return Ok(Array.Empty<BookingDto>());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id)
    {
        return NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateBookingDto dto)
    {
        return CreatedAtAction(nameof(GetById), new { id = 1 }, dto);
    }
}
