using Microsoft.EntityFrameworkCore;
using api.data;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;
using api.Services.Interfaces;

namespace api.Services;

public class JourneyPathTimePlanService : IJourneyPathTimePlanService
{
    private readonly TisDbContext _context;
    private readonly JourneyPathTimePlanMapper _journeyPathTimePlanMapper;

    public JourneyPathTimePlanService(TisDbContext context, JourneyPathTimePlanMapper journeyPathTimePlanMapper)
    {
        _context = context;
        _journeyPathTimePlanMapper = journeyPathTimePlanMapper;
    }

    public async Task<JourneyPathTimePlanOutput?> GetJourneyPathTimePlanByIdAsync(Guid journeyPathTimePlanId)
    {
        var journeyPathTimePlan = await _context.JourneyPathTimePlans.FindAsync(journeyPathTimePlanId);
        return journeyPathTimePlan != null ? _journeyPathTimePlanMapper.toOutput(journeyPathTimePlan) : null;
    }

    public async Task<List<JourneyPathTimePlanOutput>> GetAllJourneyPathTimePlansAsync()
    {
        var journeyPathTimePlans = await _context.JourneyPathTimePlans.ToListAsync();
        return journeyPathTimePlans.Select(_journeyPathTimePlanMapper.toOutput).ToList();
    }

    public async Task<JourneyPathTimePlanOutput> CreateJourneyPathTimePlanAsync(JourneyPathTimePlanInput journeyPathTimePlanInput)
    {
        var journeyPathTimePlan = _journeyPathTimePlanMapper.toEntity(journeyPathTimePlanInput);
        _context.JourneyPathTimePlans.Add(journeyPathTimePlan);
        await _context.SaveChangesAsync();
        return _journeyPathTimePlanMapper.toOutput(journeyPathTimePlan);
    }

    public async Task<JourneyPathTimePlanOutput?> UpdateJourneyPathTimePlanAsync(Guid journeyPathTimePlanId, JourneyPathTimePlanInput journeyPathTimePlanInput)
    {
        var existingJourneyPathTimePlan = await _context.JourneyPathTimePlans.FindAsync(journeyPathTimePlanId);
        if (existingJourneyPathTimePlan == null)
        {
            return null;
        }

        _journeyPathTimePlanMapper.updateEntity(journeyPathTimePlanInput, existingJourneyPathTimePlan);
        await _context.SaveChangesAsync();
        return _journeyPathTimePlanMapper.toOutput(existingJourneyPathTimePlan);
    }

    public async Task<bool> DeleteJourneyPathTimePlanAsync(Guid journeyPathTimePlanId)
    {
        var journeyPathTimePlan = await _context.JourneyPathTimePlans.FindAsync(journeyPathTimePlanId);
        if (journeyPathTimePlan == null)
        {
            return false;
        }

        _context.JourneyPathTimePlans.Remove(journeyPathTimePlan);
        await _context.SaveChangesAsync();
        return true;
    }
}
