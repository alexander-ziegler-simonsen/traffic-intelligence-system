using api.Services.Interfaces;
using api.Dtos.Input;

namespace api.Endpoints;

public static class JourneyPathTimePlanEndpoints
{
    public static void MapJourneyPathTimePlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/journey-path-time-plans").WithTags("Journey Path Time Plans");

        group.MapGet("/", GetAllJourneyPathTimePlansAsync);
        group.MapGet("/{id}", GetJourneyPathTimePlanByIdAsync);
        group.MapPost("/", CreateJourneyPathTimePlanAsync);
        group.MapPut("/{id}", UpdateJourneyPathTimePlanAsync);
        group.MapDelete("/{id}", DeleteJourneyPathTimePlanAsync);
    }

    private static async Task<IResult> GetAllJourneyPathTimePlansAsync(IJourneyPathTimePlanService journeyPathTimePlanService)
    {
        var journeyPathTimePlans = await journeyPathTimePlanService.GetAllJourneyPathTimePlansAsync();
        return Results.Ok(journeyPathTimePlans);
    }

    private static async Task<IResult> GetJourneyPathTimePlanByIdAsync(Guid id, IJourneyPathTimePlanService journeyPathTimePlanService)
    {
        var journeyPathTimePlan = await journeyPathTimePlanService.GetJourneyPathTimePlanByIdAsync(id);
        if (journeyPathTimePlan == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(journeyPathTimePlan);
    }

    private static async Task<IResult> CreateJourneyPathTimePlanAsync(JourneyPathTimePlanInput journeyPathTimePlan, IJourneyPathTimePlanService journeyPathTimePlanService)
    {
        var createdJourneyPathTimePlan = await journeyPathTimePlanService.CreateJourneyPathTimePlanAsync(journeyPathTimePlan);
        return Results.Created($"/journey-path-time-plans/{createdJourneyPathTimePlan.Id}", createdJourneyPathTimePlan);
    }

    private static async Task<IResult> UpdateJourneyPathTimePlanAsync(Guid id, JourneyPathTimePlanInput journeyPathTimePlan, IJourneyPathTimePlanService journeyPathTimePlanService)
    {
        var updatedJourneyPathTimePlan = await journeyPathTimePlanService.UpdateJourneyPathTimePlanAsync(id, journeyPathTimePlan);
        if (updatedJourneyPathTimePlan == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(updatedJourneyPathTimePlan);
    }

    private static async Task<IResult> DeleteJourneyPathTimePlanAsync(Guid id, IJourneyPathTimePlanService journeyPathTimePlanService)
    {
        var deleted = await journeyPathTimePlanService.DeleteJourneyPathTimePlanAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }
        return Results.NoContent();
    }
}
