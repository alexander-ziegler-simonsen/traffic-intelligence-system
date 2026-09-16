using Microsoft.EntityFrameworkCore;
using api.data;
using api.Interfaces;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Services;

public class JourneyService : IJourneyService
{
    private readonly TisDbContext _context;
    private readonly JourneyMapper _journeyMapper;

    public JourneyService(TisDbContext context, JourneyMapper journeyMapper)
    {
        _context = context;
        _journeyMapper = journeyMapper;
    }

    public async Task<JourneyOutput?> GetJourneyByIdAsync(Guid journeyId)
    {
        var journey = await _context.Journeys.FindAsync(journeyId);
        return journey != null ? _journeyMapper.toOutput(journey) : null;
    }

    public async Task<List<JourneyOutput>> GetAllJourneysAsync()
    {
        var journeys = await _context.Journeys.ToListAsync();
        return journeys.Select(_journeyMapper.toOutput).ToList();
    }

    public async Task<JourneyOutput> CreateJourneyAsync(JourneyInput journeyInput)
    {
        var journey = _journeyMapper.toEntity(journeyInput);
        _context.Journeys.Add(journey);
        await _context.SaveChangesAsync();
        return _journeyMapper.toOutput(journey);
    }

    public async Task<JourneyOutput?> UpdateJourneyAsync(Guid journeyId, JourneyInput journeyInput)
    {
        var existingJourney = await _context.Journeys.FindAsync(journeyId);
        if (existingJourney == null)
        {
            return null;
        }

        _journeyMapper.updateEntity(journeyInput, existingJourney);
        await _context.SaveChangesAsync();
        return _journeyMapper.toOutput(existingJourney);
    }

    public async Task<bool> DeleteJourneyAsync(Guid journeyId)
    {
        var journey = await _context.Journeys.FindAsync(journeyId);
        if (journey == null)
        {
            return false;
        }

        _context.Journeys.Remove(journey);
        await _context.SaveChangesAsync();
        return true;
    }
}
