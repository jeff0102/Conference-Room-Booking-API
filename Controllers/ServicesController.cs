using ConferenceRoomBookingApi.Application.DTOs.Service;
using ConferenceRoomBookingApi.Domain.Entities;
using ConferenceRoomBookingApi.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IServiceRepository _serviceRepository;

    public ServicesController(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var services = await _serviceRepository.GetAllAsync();
        var dtos = services.Select(s => new ServiceDto
        {
            Id = s.Id,
            Name = s.Name,
            Price = s.Price
        });

        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null)
        {
            return NotFound(new { message = $"Service with ID {id} not found." });
        }

        return Ok(new ServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Price = service.Price
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var service = new Service
        {
            Name = dto.Name,
            Price = dto.Price
        };

        var id = await _serviceRepository.CreateAsync(service);
        var createdDto = new ServiceDto
        {
            Id = id,
            Name = service.Name,
            Price = service.Price
        };

        return CreatedAtAction(nameof(GetById), new { id }, createdDto);
    }
}
