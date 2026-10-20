using api.Services.Interfaces;
using api.Dtos.Input;

namespace api.Endpoints;

public static class UsingJourneyLogEndpoints
{
    public static void MapUsingJourneyLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/using-journey-logs").WithTags("Using Journey Logs");

        group.MapGet("/", GetAllUsingJourneyLogsAsync);
        group.MapGet("/{id}", GetUsingJourneyLogByIdAsync);
        group.MapPost("/", CreateUsingJourneyLogAsync);
        group.MapPut("/{id}", UpdateUsingJourneyLogAsync);
        group.MapDelete("/{id}", DeleteUsingJourneyLogAsync);
    }

    private static async Task<IResult> GetAllUsingJourneyLogsAsync(IUsingJourneyLogService usingJourneyLogService)
    {
        var usingJourneyLogs = await usingJourneyLogService.GetAllUsingJourneyLogsAsync();
        return Results.Ok(usingJourneyLogs);
    }

    private static async Task<IResult> GetUsingJourneyLogByIdAsync(Guid id, IUsingJourneyLogService usingJourneyLogService)
    {
        var usingJourneyLog = await usingJourneyLogService.GetUsingJourneyLogByIdAsync(id);
        if (usingJourneyLog == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(usingJourneyLog);
    }

    private static async Task<IResult> CreateUsingJourneyLogAsync(UsingJourneyLogInput usingJourneyLog, IUsingJourneyLogService usingJourneyLogService)
    {
        var createdUsingJourneyLog = await usingJourneyLogService.CreateUsingJourneyLogAsync(usingJourneyLog);
        return Results.Created($"/using-journey-logs/{createdUsingJourneyLog.Id}", createdUsingJourneyLog);
    }

    private static async Task<IResult> UpdateUsingJourneyLogAsync(Guid id, UsingJourneyLogInput usingJourneyLog, IUsingJourneyLogService usingJourneyLogService)
    {
        var updatedUsingJourneyLog = await usingJourneyLogService.UpdateUsingJourneyLogAsync(id, usingJourneyLog);
        if (updatedUsingJourneyLog == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(updatedUsingJourneyLog);
    }

    private static async Task<IResult> DeleteUsingJourneyLogAsync(Guid id, IUsingJourneyLogService usingJourneyLogService)
    {
        var deleted = await usingJourneyLogService.DeleteUsingJourneyLogAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }
        return Results.NoContent();
    }
}
