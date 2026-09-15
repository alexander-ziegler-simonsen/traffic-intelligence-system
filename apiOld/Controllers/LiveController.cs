using Microsoft.AspNetCore.Mvc;
using TisApi.Services.Interfaces;

namespace TisApi.Controllers;

[ApiController]
[Route("api/live")]
public class LiveController(ILiveService live) : ControllerBase
{
    [HttpGet("cameras/{cameraId:int}")]
    public async Task<IActionResult> GetCamera(int cameraId)
    {
        var data = await live.GetCameraAsync(cameraId);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpGet("incidents/{incidentId:int}")]
    public async Task<IActionResult> GetIncident(int incidentId)
    {
        var data = await live.GetIncidentAsync(incidentId);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpGet("roads/{roadId:int}/cameras")]
    public async Task<IActionResult> GetRoadCameraIds(int roadId) =>
        Ok(await live.GetRoadCameraIdsAsync(roadId));
}
