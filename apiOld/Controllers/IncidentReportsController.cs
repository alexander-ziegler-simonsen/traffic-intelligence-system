using Microsoft.AspNetCore.Mvc;
using TisApi.Services.Interfaces;

namespace TisApi.Controllers;

[ApiController]
[Route("api/incident-reports")]
public class IncidentReportsController(IIncidentReportService reports) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? minSeverity = null,
        [FromQuery] string? roadName = null,
        [FromQuery] string? cameraLabel = null) =>
        Ok(await reports.GetAllAsync(minSeverity, roadName, cameraLabel));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var report = await reports.GetByIdAsync(id);
        return report is null ? NotFound() : Ok(report);
    }
}
