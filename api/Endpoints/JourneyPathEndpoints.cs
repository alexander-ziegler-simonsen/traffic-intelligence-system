using api.Services.Interfaces;
using api.Dtos.Input;

namespace api.Endpoints;

public static class JourneyPathEndpoints
{
    public static void MapJourneyPathEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/journey-paths").WithTags("Journey Paths");

        group.MapGet("/", GetAllJourneyPathsAsync);
        group.MapGet("/{id}", GetJourneyPathByIdAsync);
        group.MapPost("/", CreateJourneyPathAsync);
        group.MapPut("/{id}", UpdateJourneyPathAsync);
        group.MapDelete("/{id}", DeleteJourneyPathAsync);
    }

    private static async Task<IResult> GetAllJourneyPathsAsync(IJourneyPathService journeyPathService)
    {
        var journeyPaths = await journeyPathService.GetAllJourneyPathsAsync();
        return Results.Ok(journeyPaths);
    }

    private static async Task<IResult> GetJourneyPathByIdAsync(Guid id, IJourneyPathService journeyPathService)
    {
        var journeyPath = await journeyPathService.GetJourneyPathByIdAsync(id);
        if (journeyPath == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(journeyPath);
    }

    private static async Task<IResult> CreateJourneyPathAsync(JourneyPathInput journeyPath, IJourneyPathService journeyPathService)
    {
        var createdJourneyPath = await journeyPathService.CreateJourneyPathAsync(journeyPath);
        return Results.Created($"/journey-paths/{createdJourneyPath.Id}", createdJourneyPath);
    }

    private static async Task<IResult> UpdateJourneyPathAsync(Guid id, JourneyPathInput journeyPath, IJourneyPathService journeyPathService)
    {
        var updatedJourneyPath = await journeyPathService.UpdateJourneyPathAsync(id, journeyPath);
        if (updatedJourneyPath == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(updatedJourneyPath);
    }

    private static async Task<IResult> DeleteJourneyPathAsync(Guid id, IJourneyPathService journeyPathService)
    {
        var deleted = await journeyPathService.DeleteJourneyPathAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }
        return Results.NoContent();
    }
}
