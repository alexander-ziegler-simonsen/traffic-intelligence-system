using Microsoft.EntityFrameworkCore;
using api.data;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;
using api.Services.Interfaces;

namespace api.Services;

public class UsingJourneyService : IUsingJourneyService
{
    private readonly TisDbContext _context;
    private readonly UsingJourneyMapper _usingJourneyMapper;

    public UsingJourneyService(TisDbContext context, UsingJourneyMapper usingJourneyMapper)
    {
        _context = context;
        _usingJourneyMapper = usingJourneyMapper;
    }

    public async Task<UsingJourneyOutput?> GetUsingJourneyByIdAsync(Guid usingJourneyId)
    {
        var usingJourney = await _context.UsingJourneys.FindAsync(usingJourneyId);
        return usingJourney != null ? _usingJourneyMapper.toOutput(usingJourney) : null;
    }

    public async Task<List<UsingJourneyOutput>> GetAllUsingJourneysAsync()
    {
        var usingJourneys = await _context.UsingJourneys.ToListAsync();
        return usingJourneys.Select(_usingJourneyMapper.toOutput).ToList();
    }

    public async Task<UsingJourneyOutput> CreateUsingJourneyAsync(UsingJourneyInput usingJourneyInput)
    {
        var usingJourney = _usingJourneyMapper.toEntity(usingJourneyInput);
        _context.UsingJourneys.Add(usingJourney);
        await _context.SaveChangesAsync();
        return _usingJourneyMapper.toOutput(usingJourney);
    }

    public async Task<UsingJourneyOutput?> UpdateUsingJourneyAsync(Guid usingJourneyId, UsingJourneyInput usingJourneyInput)
    {
        var existingUsingJourney = await _context.UsingJourneys.FindAsync(usingJourneyId);
        if (existingUsingJourney == null)
        {
            return null;
        }

        _usingJourneyMapper.updateEntity(usingJourneyInput, existingUsingJourney);
        await _context.SaveChangesAsync();
        return _usingJourneyMapper.toOutput(existingUsingJourney);
    }

    public async Task<bool> DeleteUsingJourneyAsync(Guid usingJourneyId)
    {
        var usingJourney = await _context.UsingJourneys.FindAsync(usingJourneyId);
        if (usingJourney == null)
        {
            return false;
        }

        _context.UsingJourneys.Remove(usingJourney);
        await _context.SaveChangesAsync();
        return true;
    }
}
