using Microsoft.EntityFrameworkCore;
using api.data;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;
using api.Services.Interfaces;

namespace api.Services;

public class VehicleTypeService : IVehicleTypeService
{
    private readonly TisDbContext _context;
    private readonly VehicleTypeMapper _vehicleTypeMapper;

    public VehicleTypeService(TisDbContext context, VehicleTypeMapper vehicleTypeMapper)
    {
        _context = context;
        _vehicleTypeMapper = vehicleTypeMapper;
    }

    public async Task<VehicleTypeOutput?> GetVehicleTypeByIdAsync(Guid vehicleTypeId)
    {
        var vehicleType = await _context.VehicleTypes.FindAsync(vehicleTypeId);
        return vehicleType != null ? _vehicleTypeMapper.toOutput(vehicleType) : null;
    }

    public async Task<List<VehicleTypeOutput>> GetAllVehicleTypesAsync()
    {
        var vehicleTypes = await _context.VehicleTypes.ToListAsync();
        return vehicleTypes.Select(_vehicleTypeMapper.toOutput).ToList();
    }

    public async Task<VehicleTypeOutput> CreateVehicleTypeAsync(VehicleTypeInput vehicleTypeInput)
    {
        var vehicleType = _vehicleTypeMapper.toEntity(vehicleTypeInput);
        _context.VehicleTypes.Add(vehicleType);
        await _context.SaveChangesAsync();
        return _vehicleTypeMapper.toOutput(vehicleType);
    }

    public async Task<VehicleTypeOutput?> UpdateVehicleTypeAsync(Guid vehicleTypeId, VehicleTypeInput vehicleTypeInput)
    {
        var existingVehicleType = await _context.VehicleTypes.FindAsync(vehicleTypeId);
        if (existingVehicleType == null)
        {
            return null;
        }

        _vehicleTypeMapper.updateEntity(vehicleTypeInput, existingVehicleType);
        await _context.SaveChangesAsync();
        return _vehicleTypeMapper.toOutput(existingVehicleType);
    }

    public async Task<bool> DeleteVehicleTypeAsync(Guid vehicleTypeId)
    {
        var vehicleType = await _context.VehicleTypes.FindAsync(vehicleTypeId);
        if (vehicleType == null)
        {
            return false;
        }

        _context.VehicleTypes.Remove(vehicleType);
        await _context.SaveChangesAsync();
        return true;
    }
}
