using Microsoft.AspNetCore.Mvc;
using TisApi.Models;
using TisApi.Services.Interfaces;

namespace TisApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoadsController(IRoadService roads) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await roads.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var road = await roads.GetByIdAsync(id);
        return road is null ? NotFound() : Ok(road);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoadRequest request)
    {
        var road = await roads.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = road.Id }, road);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRoadRequest request)
    {
        var road = await roads.UpdateAsync(id, request);
        return road is null ? NotFound() : Ok(road);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await roads.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
