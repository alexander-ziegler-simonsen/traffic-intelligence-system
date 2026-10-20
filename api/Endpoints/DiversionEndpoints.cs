using api.Services.Interfaces;

namespace api.Endpoints;

public static class DiversionEndpoints
{
    public static void MapDiversionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/diversions").WithTags("Diversions");

        // bus lines currently (or soon) driving around a disruption; they're made when a disruption is saved
        group.MapGet("/", GetDiversionsAsync);
    }

    private static async Task<IResult> GetDiversionsAsync(IDiversionService diversionService)
    {
        var diversions = await diversionService.GetDiversionsAsync();
        return Results.Ok(diversions);
    }
}
