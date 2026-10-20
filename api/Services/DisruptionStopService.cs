using Microsoft.EntityFrameworkCore;
using api.data;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;
using api.Services.Interfaces;

namespace api.Services;

public class DisruptionStopService : IDisruptionStopService
{
    private readonly TisDbContext _context;
    private readonly DisruptionStopMapper _disruptionStopMapper;

    public DisruptionStopService(TisDbContext context, DisruptionStopMapper disruptionStopMapper)
    {
        _context = context;
        _disruptionStopMapper = disruptionStopMapper;
    }

    public async Task<DisruptionStopOutput?> GetDisruptionStopByIdAsync(Guid disruptionStopId)
    {
        var disruptionStop = await _context.DisruptionStops.FindAsync(disruptionStopId);
        return disruptionStop != null ? _disruptionStopMapper.toOutput(disruptionStop) : null;
    }

    public async Task<List<DisruptionStopOutput>> GetAllDisruptionStopsAsync()
    {
        var disruptionStops = await _context.DisruptionStops.ToListAsync();
        return disruptionStops.Select(_disruptionStopMapper.toOutput).ToList();
    }

    public async Task<DisruptionStopOutput> CreateDisruptionStopAsync(DisruptionStopInput disruptionStopInput)
    {
        var disruptionStop = _disruptionStopMapper.toEntity(disruptionStopInput);
        _context.DisruptionStops.Add(disruptionStop);
        await _context.SaveChangesAsync();
        return _disruptionStopMapper.toOutput(disruptionStop);
    }

    public async Task<DisruptionStopOutput?> UpdateDisruptionStopAsync(Guid disruptionStopId, DisruptionStopInput disruptionStopInput)
    {
        var existingDisruptionStop = await _context.DisruptionStops.FindAsync(disruptionStopId);
        if (existingDisruptionStop == null)
        {
            return null;
        }

        _disruptionStopMapper.updateEntity(disruptionStopInput, existingDisruptionStop);
        await _context.SaveChangesAsync();
        return _disruptionStopMapper.toOutput(existingDisruptionStop);
    }

    public async Task<bool> DeleteDisruptionStopAsync(Guid disruptionStopId)
    {
        var disruptionStop = await _context.DisruptionStops.FindAsync(disruptionStopId);
        if (disruptionStop == null)
        {
            return false;
        }

        _context.DisruptionStops.Remove(disruptionStop);
        await _context.SaveChangesAsync();
        return true;
    }
}
