using Microsoft.AspNetCore.Mvc;
using WebApplication1.Database;
using WebApplication1.DTO;
using WebApplication1.Exceptions;


namespace WebApplication1.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IDbService _dbService;

    public AppointmentsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        try
        {
            var result = await _dbService.GetAppointmentByIdAsync(id);
            return Ok(result);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddAppointment(CreateAppointmentDto dto)
    {
        try
        {
            await _dbService.AddAppointmentAsync(dto);
            return CreatedAtAction(nameof(GetAppointment), new { id = dto.AppointmentId }, null);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
