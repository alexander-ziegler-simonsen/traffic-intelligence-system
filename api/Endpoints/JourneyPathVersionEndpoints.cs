using api.Services.Interfaces;
using api.Dtos.Input;

namespace api.Endpoints;

public static class JourneyPathVersionEndpoints
{
    public static void MapJourneyPathVersionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/journey-path-versions").WithTags("Journey Path Versions");

        group.MapGet("/", GetAllJourneyPathVersionsAsync);
        group.MapGet("/{id}", GetJourneyPathVersionByIdAsync);
        group.MapPost("/", CreateJourneyPathVersionAsync);
        group.MapPut("/{id}", UpdateJourneyPathVersionAsync);
        group.MapDelete("/{id}", DeleteJourneyPathVersionAsync);
    }

    private static async Task<IResult> GetAllJourneyPathVersionsAsync(IJourneyPathVersionService journeyPathVersionService)
    {
        var journeyPathVersions = await journeyPathVersionService.GetAllJourneyPathVersionsAsync();
        return Results.Ok(journeyPathVersions);
    }

    private static async Task<IResult> GetJourneyPathVersionByIdAsync(Guid id, IJourneyPathVersionService journeyPathVersionService)
    {
        var journeyPathVersion = await journeyPathVersionService.GetJourneyPathVersionByIdAsync(id);
        if (journeyPathVersion == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(journeyPathVersion);
    }

    private static async Task<IResult> CreateJourneyPathVersionAsync(JourneyPathVersionInput journeyPathVersion, IJourneyPathVersionService journeyPathVersionService)
    {
        var createdJourneyPathVersion = await journeyPathVersionService.CreateJourneyPathVersionAsync(journeyPathVersion);
        return Results.Created($"/journey-path-versions/{createdJourneyPathVersion.Id}", createdJourneyPathVersion);
    }

    private static async Task<IResult> UpdateJourneyPathVersionAsync(Guid id, JourneyPathVersionInput journeyPathVersion, IJourneyPathVersionService journeyPathVersionService)
    {
        var updatedJourneyPathVersion = await journeyPathVersionService.UpdateJourneyPathVersionAsync(id, journeyPathVersion);
        if (updatedJourneyPathVersion == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(updatedJourneyPathVersion);
    }

    private static async Task<IResult> DeleteJourneyPathVersionAsync(Guid id, IJourneyPathVersionService journeyPathVersionService)
    {
        var deleted = await journeyPathVersionService.DeleteJourneyPathVersionAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }
        return Results.NoContent();
    }
}
