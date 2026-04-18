using Microsoft.AspNetCore.Mvc;
using TisApi.Models;
using TisApi.Services.Interfaces;

namespace TisApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CamerasController(ICameraService cameras) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? roadId) =>
        Ok(await cameras.GetAllAsync(roadId));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var camera = await cameras.GetByIdAsync(id);
        return camera is null ? NotFound() : Ok(camera);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCameraRequest request)
    {
        var camera = await cameras.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = camera.Id }, camera);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCameraRequest request)
    {
        var camera = await cameras.UpdateAsync(id, request);
        return camera is null ? NotFound() : Ok(camera);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await cameras.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
