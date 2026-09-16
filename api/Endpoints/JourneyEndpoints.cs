using api.Interfaces;
using api.Dtos.Input;

namespace api.Endpoints;

public static class JourneyEndpoints
{
    public static void MapJourneyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/journeys").WithTags("Journeys");

        group.MapGet("/", GetAllJourneysAsync);
        group.MapGet("/{id}", GetJourneyByIdAsync);
        group.MapPost("/", CreateJourneyAsync);
        group.MapPut("/{id}", UpdateJourneyAsync);
        group.MapDelete("/{id}", DeleteJourneyAsync);
    }

    private static async Task<IResult> GetAllJourneysAsync(IJourneyService journeyService)
    {
        var journeys = await journeyService.GetAllJourneysAsync();
        return Results.Ok(journeys);
    }

    private static async Task<IResult> GetJourneyByIdAsync(Guid id, IJourneyService journeyService)
    {
        var journey = await journeyService.GetJourneyByIdAsync(id);
        if (journey == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(journey);
    }

    private static async Task<IResult> CreateJourneyAsync(JourneyInput journey, IJourneyService journeyService)
    {
        var createdJourney = await journeyService.CreateJourneyAsync(journey);
        return Results.Created($"/journeys/{createdJourney.Id}", createdJourney);
    }

    private static async Task<IResult> UpdateJourneyAsync(Guid id, JourneyInput journey, IJourneyService journeyService)
    {
        var updatedJourney = await journeyService.UpdateJourneyAsync(id, journey);
        if (updatedJourney == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(updatedJourney);
    }

    private static async Task<IResult> DeleteJourneyAsync(Guid id, IJourneyService journeyService)
    {
        var deleted = await journeyService.DeleteJourneyAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }
        return Results.NoContent();
    }
}
