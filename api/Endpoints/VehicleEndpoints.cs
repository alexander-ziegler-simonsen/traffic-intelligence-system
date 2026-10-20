using api.Services.Interfaces;
using api.Dtos.Input;

namespace api.Endpoints;

public static class VehicleEndpoints
{
    public static void MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/vehicles").WithTags("Vehicles");

        group.MapGet("/", GetAllVehiclesAsync);
        group.MapGet("/{id}", GetVehicleByIdAsync);
        group.MapPost("/", CreateVehicleAsync);
        group.MapPut("/{id}", UpdateVehicleAsync);
        group.MapDelete("/{id}", DeleteVehicleAsync);
    }

    private static async Task<IResult> GetAllVehiclesAsync(IVehicleService vehicleService)
    {
        var vehicles = await vehicleService.GetAllVehiclesAsync();
        return Results.Ok(vehicles);
    }

    private static async Task<IResult> GetVehicleByIdAsync(Guid id, IVehicleService vehicleService)
    {
        var vehicle = await vehicleService.GetVehicleByIdAsync(id);
        if (vehicle == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(vehicle);
    }

    private static async Task<IResult> CreateVehicleAsync(VehicleInput vehicle, IVehicleService vehicleService)
    {
        var createdVehicle = await vehicleService.CreateVehicleAsync(vehicle);
        return Results.Created($"/vehicles/{createdVehicle.Id}", createdVehicle);
    }

    private static async Task<IResult> UpdateVehicleAsync(Guid id, VehicleInput vehicle, IVehicleService vehicleService)
    {
        var updatedVehicle = await vehicleService.UpdateVehicleAsync(id, vehicle);
        if (updatedVehicle == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(updatedVehicle);
    }

    private static async Task<IResult> DeleteVehicleAsync(Guid id, IVehicleService vehicleService)
    {
        var deleted = await vehicleService.DeleteVehicleAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }
        return Results.NoContent();
    }
}
