using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Services.Interfaces;

public interface IVehicleTypeService
{
    public Task<VehicleTypeOutput?> GetVehicleTypeByIdAsync(Guid vehicleTypeId);
    public Task<List<VehicleTypeOutput>> GetAllVehicleTypesAsync();
    public Task<VehicleTypeOutput> CreateVehicleTypeAsync(VehicleTypeInput vehicleTypeInput);
    public Task<VehicleTypeOutput?> UpdateVehicleTypeAsync(Guid vehicleTypeId, VehicleTypeInput vehicleTypeInput);
    public Task<bool> DeleteVehicleTypeAsync(Guid vehicleTypeId);
}
