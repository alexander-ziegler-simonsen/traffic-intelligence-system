using Microsoft.EntityFrameworkCore;
using api.data;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;
using api.Services.Interfaces;

namespace api.Services;

public class DisruptionService : IDisruptionService
{
    private readonly TisDbContext _context;
    private readonly DisruptionMapper _disruptionMapper;

    public DisruptionService(TisDbContext context, DisruptionMapper disruptionMapper)
    {
        _context = context;
        _disruptionMapper = disruptionMapper;
    }

    public async Task<DisruptionOutput?> GetDisruptionByIdAsync(Guid disruptionId)
    {
        var disruption = await _context.Disruptions.FindAsync(disruptionId);
        return disruption != null ? _disruptionMapper.toOutput(disruption) : null;
    }

    public async Task<List<DisruptionOutput>> GetAllDisruptionsAsync()
    {
        var disruptions = await _context.Disruptions.ToListAsync();
        return disruptions.Select(_disruptionMapper.toOutput).ToList();
    }

    public async Task<DisruptionOutput> CreateDisruptionAsync(DisruptionInput disruptionInput)
    {
        var disruption = _disruptionMapper.toEntity(disruptionInput);
        _context.Disruptions.Add(disruption);
        await _context.SaveChangesAsync();
        return _disruptionMapper.toOutput(disruption);
    }

    public async Task<DisruptionOutput?> UpdateDisruptionAsync(Guid disruptionId, DisruptionInput disruptionInput)
    {
        var existingDisruption = await _context.Disruptions.FindAsync(disruptionId);
        if (existingDisruption == null)
        {
            return null;
        }

        _disruptionMapper.updateEntity(disruptionInput, existingDisruption);
        await _context.SaveChangesAsync();
        return _disruptionMapper.toOutput(existingDisruption);
    }

    public async Task<bool> DeleteDisruptionAsync(Guid disruptionId)
    {
        var disruption = await _context.Disruptions.FindAsync(disruptionId);
        if (disruption == null)
        {
            return false;
        }

        _context.Disruptions.Remove(disruption);
        await _context.SaveChangesAsync();
        return true;
    }
}
