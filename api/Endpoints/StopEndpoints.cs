using api.Services.Interfaces;
using api.Dtos.Input;

namespace api.Endpoints;

public static class StopEndpoints
{
    public static void MapStopEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/stops").WithTags("Stops");

        group.MapGet("/", GetAllStopsAsync);
        group.MapGet("/{id}", GetStopByIdAsync);
        group.MapPost("/", CreateStopAsync);
        group.MapPut("/{id}", UpdateStopAsync);
        group.MapDelete("/{id}", DeleteStopAsync);
    }

    private static async Task<IResult> GetAllStopsAsync(IStopService stopService)
    {
        var stops = await stopService.GetAllStopsAsync();
        return Results.Ok(stops);
    }

    private static async Task<IResult> GetStopByIdAsync(Guid id, IStopService stopService)
    {
        var stop = await stopService.GetStopByIdAsync(id);
        if (stop == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(stop);
    }

    private static async Task<IResult> CreateStopAsync(StopInput stop, IStopService stopService)
    {
        var createdStop = await stopService.CreateStopAsync(stop);
        return Results.Created($"/stops/{createdStop.Id}", createdStop);
    }

    private static async Task<IResult> UpdateStopAsync(Guid id, StopInput stop, IStopService stopService)
    {
        var updatedStop = await stopService.UpdateStopAsync(id, stop);
        if (updatedStop == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(updatedStop);
    }

    private static async Task<IResult> DeleteStopAsync(Guid id, IStopService stopService)
    {
        var deleted = await stopService.DeleteStopAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }
        return Results.NoContent();
    }
}
