using Microsoft.AspNetCore.Mvc;
using TisApi.Simulation;

namespace TisApi.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController(SimulatorState state) : ControllerBase
{
    [HttpGet("geojson")]
    public ContentResult GetGeoJson() =>
        Content(state.GeoJson, "application/json");
}
