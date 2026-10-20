using Microsoft.EntityFrameworkCore;
using api.data;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;
using api.Services.Interfaces;

namespace api.Services;

public class StopService : IStopService
{
    private readonly TisDbContext _context;
    private readonly StopMapper _stopMapper;

    public StopService(TisDbContext context, StopMapper stopMapper)
    {
        _context = context;
        _stopMapper = stopMapper;
    }

    public async Task<StopOutput?> GetStopByIdAsync(Guid stopId)
    {
        var stop = await _context.Stops.FindAsync(stopId);
        return stop != null ? _stopMapper.toOutput(stop) : null;
    }

    public async Task<List<StopOutput>> GetAllStopsAsync()
    {
        var stops = await _context.Stops.ToListAsync();
        return stops.Select(_stopMapper.toOutput).ToList();
    }

    public async Task<StopOutput> CreateStopAsync(StopInput stopInput)
    {
        var stop = _stopMapper.toEntity(stopInput);
        _context.Stops.Add(stop);
        await _context.SaveChangesAsync();
        return _stopMapper.toOutput(stop);
    }

    public async Task<StopOutput?> UpdateStopAsync(Guid stopId, StopInput stopInput)
    {
        var existingStop = await _context.Stops.FindAsync(stopId);
        if (existingStop == null)
        {
            return null;
        }

        _stopMapper.updateEntity(stopInput, existingStop);
        await _context.SaveChangesAsync();
        return _stopMapper.toOutput(existingStop);
    }

    public async Task<bool> DeleteStopAsync(Guid stopId)
    {
        var stop = await _context.Stops.FindAsync(stopId);
        if (stop == null)
        {
            return false;
        }

        _context.Stops.Remove(stop);
        await _context.SaveChangesAsync();
        return true;
    }
}
