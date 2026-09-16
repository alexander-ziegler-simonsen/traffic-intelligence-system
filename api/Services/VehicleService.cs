using Microsoft.EntityFrameworkCore;
using api.data;
using api.Interfaces;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Services;

public class VehicleService : IVehicleService
{
    private readonly TisDbContext _context;
    private readonly VehicleMapper _vehicleMapper;

    public VehicleService(TisDbContext context, VehicleMapper vehicleMapper)
    {
        _context = context;
        _vehicleMapper = vehicleMapper;
    }

    public async Task<VehicleOutput?> GetVehicleByIdAsync(Guid vehicleId)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        return vehicle != null ? _vehicleMapper.toOutput(vehicle) : null;
    }
    public async Task<List<VehicleOutput>> GetAllVehiclesAsync()
    {
        var vehicles = await _context.Vehicles.ToListAsync();
        return vehicles.Select(_vehicleMapper.toOutput).ToList();
    }
    public async Task<VehicleOutput> CreateVehicleAsync(VehicleInput vehicleInput)
    {
        var vehicle = _vehicleMapper.toEntity(vehicleInput);
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        return _vehicleMapper.toOutput(vehicle);
    }
    public async Task<VehicleOutput?> UpdateVehicleAsync(Guid vehicleId, VehicleInput vehicleInput)
    {
        var existingVehicle = await _context.Vehicles.FindAsync(vehicleId);
        if (existingVehicle == null)
        {
            return null;
        }

        _vehicleMapper.updateEntity(vehicleInput, existingVehicle);
        await _context.SaveChangesAsync();
        return _vehicleMapper.toOutput(existingVehicle);
    }
    public async Task<bool> DeleteVehicleAsync(Guid vehicleId)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        if (vehicle == null)
        {
            return false;
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();
        return true;
    }
}
