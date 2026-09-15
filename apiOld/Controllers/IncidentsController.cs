using Microsoft.AspNetCore.Mvc;
using TisApi.Models;
using TisApi.Services.Interfaces;

namespace TisApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController(IIncidentService incidents) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? type = null,
        [FromQuery] string? status = null) =>
        Ok(await incidents.GetAllAsync(type, status));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var incident = await incidents.GetByIdAsync(id);
        return incident is null ? NotFound() : Ok(incident);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateIncidentRequest request)
    {
        var incident = await incidents.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = incident.Id }, incident);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await incidents.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
