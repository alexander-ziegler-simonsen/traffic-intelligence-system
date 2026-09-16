using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Interfaces;

public interface IVehicleService
{
    public Task<VehicleOutput?> GetVehicleByIdAsync(Guid vehicleId);
    public Task<List<VehicleOutput>> GetAllVehiclesAsync();
    public Task<VehicleOutput> CreateVehicleAsync(VehicleInput vehicleInput);
    public Task<VehicleOutput?> UpdateVehicleAsync(Guid vehicleId, VehicleInput vehicleInput);
    public Task<bool> DeleteVehicleAsync(Guid vehicleId);
}
