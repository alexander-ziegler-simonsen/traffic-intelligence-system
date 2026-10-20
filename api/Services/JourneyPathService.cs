using Microsoft.EntityFrameworkCore;
using api.data;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;
using api.Services.Interfaces;

namespace api.Services;

public class JourneyPathService : IJourneyPathService
{
    private readonly TisDbContext _context;
    private readonly JourneyPathMapper _journeyPathMapper;

    public JourneyPathService(TisDbContext context, JourneyPathMapper journeyPathMapper)
    {
        _context = context;
        _journeyPathMapper = journeyPathMapper;
    }

    public async Task<JourneyPathOutput?> GetJourneyPathByIdAsync(Guid journeyPathId)
    {
        var journeyPath = await _context.JourneyPaths.FindAsync(journeyPathId);
        return journeyPath != null ? _journeyPathMapper.toOutput(journeyPath) : null;
    }

    public async Task<List<JourneyPathOutput>> GetAllJourneyPathsAsync()
    {
        var journeyPaths = await _context.JourneyPaths.ToListAsync();
        return journeyPaths.Select(_journeyPathMapper.toOutput).ToList();
    }

    public async Task<JourneyPathOutput> CreateJourneyPathAsync(JourneyPathInput journeyPathInput)
    {
        var journeyPath = _journeyPathMapper.toEntity(journeyPathInput);
        _context.JourneyPaths.Add(journeyPath);
        await _context.SaveChangesAsync();
        return _journeyPathMapper.toOutput(journeyPath);
    }

    public async Task<JourneyPathOutput?> UpdateJourneyPathAsync(Guid journeyPathId, JourneyPathInput journeyPathInput)
    {
        var existingJourneyPath = await _context.JourneyPaths.FindAsync(journeyPathId);
        if (existingJourneyPath == null)
        {
            return null;
        }

        _journeyPathMapper.updateEntity(journeyPathInput, existingJourneyPath);
        await _context.SaveChangesAsync();
        return _journeyPathMapper.toOutput(existingJourneyPath);
    }

    public async Task<bool> DeleteJourneyPathAsync(Guid journeyPathId)
    {
        var journeyPath = await _context.JourneyPaths.FindAsync(journeyPathId);
        if (journeyPath == null)
        {
            return false;
        }

        _context.JourneyPaths.Remove(journeyPath);
        await _context.SaveChangesAsync();
        return true;
    }
}
