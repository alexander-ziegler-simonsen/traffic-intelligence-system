using api.Services.Interfaces;
using api.Dtos.Input;

namespace api.Endpoints;

public static class UsingJourneyEndpoints
{
    public static void MapUsingJourneyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/using-journeys").WithTags("Using Journeys");

        group.MapGet("/", GetAllUsingJourneysAsync);
        group.MapGet("/{id}", GetUsingJourneyByIdAsync);
        group.MapPost("/", CreateUsingJourneyAsync);
        group.MapPut("/{id}", UpdateUsingJourneyAsync);
        group.MapDelete("/{id}", DeleteUsingJourneyAsync);
    }

    private static async Task<IResult> GetAllUsingJourneysAsync(IUsingJourneyService usingJourneyService)
    {
        var usingJourneys = await usingJourneyService.GetAllUsingJourneysAsync();
        return Results.Ok(usingJourneys);
    }

    private static async Task<IResult> GetUsingJourneyByIdAsync(Guid id, IUsingJourneyService usingJourneyService)
    {
        var usingJourney = await usingJourneyService.GetUsingJourneyByIdAsync(id);
        if (usingJourney == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(usingJourney);
    }

    private static async Task<IResult> CreateUsingJourneyAsync(UsingJourneyInput usingJourney, IUsingJourneyService usingJourneyService)
    {
        var createdUsingJourney = await usingJourneyService.CreateUsingJourneyAsync(usingJourney);
        return Results.Created($"/using-journeys/{createdUsingJourney.Id}", createdUsingJourney);
    }

    private static async Task<IResult> UpdateUsingJourneyAsync(Guid id, UsingJourneyInput usingJourney, IUsingJourneyService usingJourneyService)
    {
        var updatedUsingJourney = await usingJourneyService.UpdateUsingJourneyAsync(id, usingJourney);
        if (updatedUsingJourney == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(updatedUsingJourney);
    }

    private static async Task<IResult> DeleteUsingJourneyAsync(Guid id, IUsingJourneyService usingJourneyService)
    {
        var deleted = await usingJourneyService.DeleteUsingJourneyAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }
        return Results.NoContent();
    }
}
