using api.Services.Interfaces;
using api.Dtos.Input;

namespace api.Endpoints;

public static class VehicleTypeEndpoints
{
    public static void MapVehicleTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/vehicle-types").WithTags("Vehicle Types");

        group.MapGet("/", GetAllVehicleTypesAsync);
        group.MapGet("/{id}", GetVehicleTypeByIdAsync);
        group.MapPost("/", CreateVehicleTypeAsync);
        group.MapPut("/{id}", UpdateVehicleTypeAsync);
        group.MapDelete("/{id}", DeleteVehicleTypeAsync);
    }

    private static async Task<IResult> GetAllVehicleTypesAsync(IVehicleTypeService vehicleTypeService)
    {
        var vehicleTypes = await vehicleTypeService.GetAllVehicleTypesAsync();
        return Results.Ok(vehicleTypes);
    }

    private static async Task<IResult> GetVehicleTypeByIdAsync(Guid id, IVehicleTypeService vehicleTypeService)
    {
        var vehicleType = await vehicleTypeService.GetVehicleTypeByIdAsync(id);
        if (vehicleType == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(vehicleType);
    }

    private static async Task<IResult> CreateVehicleTypeAsync(VehicleTypeInput vehicleType, IVehicleTypeService vehicleTypeService)
    {
        var createdVehicleType = await vehicleTypeService.CreateVehicleTypeAsync(vehicleType);
        return Results.Created($"/vehicle-types/{createdVehicleType.Id}", createdVehicleType);
    }

    private static async Task<IResult> UpdateVehicleTypeAsync(Guid id, VehicleTypeInput vehicleType, IVehicleTypeService vehicleTypeService)
    {
        var updatedVehicleType = await vehicleTypeService.UpdateVehicleTypeAsync(id, vehicleType);
        if (updatedVehicleType == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(updatedVehicleType);
    }

    private static async Task<IResult> DeleteVehicleTypeAsync(Guid id, IVehicleTypeService vehicleTypeService)
    {
        var deleted = await vehicleTypeService.DeleteVehicleTypeAsync(id);
        if (!deleted)
        {
            return Results.NotFound();
        }
        return Results.NoContent();
    }
}
