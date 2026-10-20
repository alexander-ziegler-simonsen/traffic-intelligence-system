using api.Services.Interfaces;
using api.Dtos.Input;

namespace api.Endpoints;

public static class DisruptionEndpoints
{
    public static void MapDisruptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/disruptions").WithTags("Disruptions");

        group.MapGet("/", GetAllDisruptionsAsync);
        group.MapGet("/{id}", GetDisruptionByIdAsync);
        group.MapPost("/", CreateDisruptionAsync);
        // what a disruption would block, without saving it
        group.MapPost("/impact", GetDisruptionImpactAsync);
        // works out the bus diversions again, e.g. if Valhalla wasn't running when the disruption was saved
        group.MapPost("/{id}/reroute", RerouteDisruptionAsync);
        group.MapPut("/{id}", UpdateDisruptionAsync);
        group.MapDelete("/{id}", DeleteDisruptionAsync);
    }

    private static async Task<IResult> RerouteDisruptionAsync(Guid id, IDiversionService diversionService)
    {
        try
        {
            var diversions = await diversionService.RerouteAsync(id);
            return diversions == null ? Results.NotFound() : Results.Ok(diversions);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return Results.Problem("Couldn't reach the Valhalla routing service", statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static async Task<IResult> GetAllDisruptionsAsync(IDisruptionService disruptionService)
    {
        var disruptions = await disruptionService.GetAllDisruptionsAsync();
        return Results.Ok(disruptions);
    }

    private static async Task<IResult> GetDisruptionByIdAsync(Guid id, IDisruptionService disruptionService)
    {
        var disruption = await disruptionService.GetDisruptionByIdAsync(id);
        if (disruption == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(disruption);
    }

    private static async Task<IResult> CreateDisruptionAsync(DisruptionInput disruption, IDisruptionService disruptionService)
    {
        var createdDisruption = await disruptionService.CreateDisruptionAsync(disruption);
        return Results.Created($"/disruptions/{createdDisruption.Id}", createdDisruption);
    }

    private static async Task<IResult> GetDisruptionImpactAsync(DisruptionInput disruption, IDisruptionService disruptionService)
    {
        var impact = await disruptionService.GetDisruptionImpactAsync(disruption);
        return Results.Ok(impact);
    }

    private static async Task<IResult> UpdateDisruptionAsync(Guid id, DisruptionInput disruption, IDisruptionService disruptionService)
    {
        var updatedDisruption = await disruptionService.UpdateDisruptionAsync(id, disruption);
        if (updatedDisruption == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(updatedDisruption);
    }

    private static async Task<IResult> DeleteDisruptionAsync(Guid id, IDisruptionService disruptionService)
    {
        var deleted = await disruptionService.DeleteDisruptionAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }
        return Results.NoContent();
    }
}
