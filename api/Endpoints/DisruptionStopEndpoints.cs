using api.Services.Interfaces;
using api.Dtos.Input;

namespace api.Endpoints;

public static class DisruptionStopEndpoints
{
    public static void MapDisruptionStopEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/disruption-stops").WithTags("Disruption Stops");

        group.MapGet("/", GetAllDisruptionStopsAsync);
        group.MapGet("/{id}", GetDisruptionStopByIdAsync);
        group.MapPost("/", CreateDisruptionStopAsync);
        group.MapPut("/{id}", UpdateDisruptionStopAsync);
        group.MapDelete("/{id}", DeleteDisruptionStopAsync);
    }

    private static async Task<IResult> GetAllDisruptionStopsAsync(IDisruptionStopService disruptionStopService)
    {
        var disruptionStops = await disruptionStopService.GetAllDisruptionStopsAsync();
        return Results.Ok(disruptionStops);
    }

    private static async Task<IResult> GetDisruptionStopByIdAsync(Guid id, IDisruptionStopService disruptionStopService)
    {
        var disruptionStop = await disruptionStopService.GetDisruptionStopByIdAsync(id);
        if (disruptionStop == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(disruptionStop);
    }

    private static async Task<IResult> CreateDisruptionStopAsync(DisruptionStopInput disruptionStop, IDisruptionStopService disruptionStopService)
    {
        var createdDisruptionStop = await disruptionStopService.CreateDisruptionStopAsync(disruptionStop);
        return Results.Created($"/disruption-stops/{createdDisruptionStop.Id}", createdDisruptionStop);
    }

    private static async Task<IResult> UpdateDisruptionStopAsync(Guid id, DisruptionStopInput disruptionStop, IDisruptionStopService disruptionStopService)
    {
        var updatedDisruptionStop = await disruptionStopService.UpdateDisruptionStopAsync(id, disruptionStop);
        if (updatedDisruptionStop == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(updatedDisruptionStop);
    }

    private static async Task<IResult> DeleteDisruptionStopAsync(Guid id, IDisruptionStopService disruptionStopService)
    {
        var deleted = await disruptionStopService.DeleteDisruptionStopAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }
        return Results.NoContent();
    }
}
