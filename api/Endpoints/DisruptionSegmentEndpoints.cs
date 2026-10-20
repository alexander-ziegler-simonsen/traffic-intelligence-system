using api.Services.Interfaces;
using api.Dtos.Input;

namespace api.Endpoints;

public static class DisruptionSegmentEndpoints
{
    public static void MapDisruptionSegmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/disruption-segments").WithTags("Disruption Segments");

        group.MapGet("/", GetAllDisruptionSegmentsAsync);
        group.MapGet("/{id}", GetDisruptionSegmentByIdAsync);
        group.MapPost("/", CreateDisruptionSegmentAsync);
        group.MapPut("/{id}", UpdateDisruptionSegmentAsync);
        group.MapDelete("/{id}", DeleteDisruptionSegmentAsync);
    }

    private static async Task<IResult> GetAllDisruptionSegmentsAsync(IDisruptionSegmentService disruptionSegmentService)
    {
        var disruptionSegments = await disruptionSegmentService.GetAllDisruptionSegmentsAsync();
        return Results.Ok(disruptionSegments);
    }

    private static async Task<IResult> GetDisruptionSegmentByIdAsync(Guid id, IDisruptionSegmentService disruptionSegmentService)
    {
        var disruptionSegment = await disruptionSegmentService.GetDisruptionSegmentByIdAsync(id);
        if (disruptionSegment == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(disruptionSegment);
    }

    private static async Task<IResult> CreateDisruptionSegmentAsync(DisruptionSegmentInput disruptionSegment, IDisruptionSegmentService disruptionSegmentService)
    {
        var createdDisruptionSegment = await disruptionSegmentService.CreateDisruptionSegmentAsync(disruptionSegment);
        return Results.Created($"/disruption-segments/{createdDisruptionSegment.Id}", createdDisruptionSegment);
    }

    private static async Task<IResult> UpdateDisruptionSegmentAsync(Guid id, DisruptionSegmentInput disruptionSegment, IDisruptionSegmentService disruptionSegmentService)
    {
        var updatedDisruptionSegment = await disruptionSegmentService.UpdateDisruptionSegmentAsync(id, disruptionSegment);
        if (updatedDisruptionSegment == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(updatedDisruptionSegment);
    }

    private static async Task<IResult> DeleteDisruptionSegmentAsync(Guid id, IDisruptionSegmentService disruptionSegmentService)
    {
        var deleted = await disruptionSegmentService.DeleteDisruptionSegmentAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }
        return Results.NoContent();
    }
}
