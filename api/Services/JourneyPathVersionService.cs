using Microsoft.EntityFrameworkCore;
using api.data;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;
using api.Services.Interfaces;

namespace api.Services;

public class JourneyPathVersionService : IJourneyPathVersionService
{
    private readonly TisDbContext _context;
    private readonly JourneyPathVersionMapper _journeyPathVersionMapper;

    public JourneyPathVersionService(TisDbContext context, JourneyPathVersionMapper journeyPathVersionMapper)
    {
        _context = context;
        _journeyPathVersionMapper = journeyPathVersionMapper;
    }

    public async Task<JourneyPathVersionOutput?> GetJourneyPathVersionByIdAsync(Guid journeyPathVersionId)
    {
        var journeyPathVersion = await _context.JourneyPathVersions.FindAsync(journeyPathVersionId);
        return journeyPathVersion != null ? _journeyPathVersionMapper.toOutput(journeyPathVersion) : null;
    }

    public async Task<List<JourneyPathVersionOutput>> GetAllJourneyPathVersionsAsync()
    {
        var journeyPathVersions = await _context.JourneyPathVersions.ToListAsync();
        return journeyPathVersions.Select(_journeyPathVersionMapper.toOutput).ToList();
    }

    public async Task<JourneyPathVersionOutput> CreateJourneyPathVersionAsync(JourneyPathVersionInput journeyPathVersionInput)
    {
        var journeyPathVersion = _journeyPathVersionMapper.toEntity(journeyPathVersionInput);
        _context.JourneyPathVersions.Add(journeyPathVersion);
        await _context.SaveChangesAsync();
        return _journeyPathVersionMapper.toOutput(journeyPathVersion);
    }

    public async Task<JourneyPathVersionOutput?> UpdateJourneyPathVersionAsync(Guid journeyPathVersionId, JourneyPathVersionInput journeyPathVersionInput)
    {
        var existingJourneyPathVersion = await _context.JourneyPathVersions.FindAsync(journeyPathVersionId);
        if (existingJourneyPathVersion == null)
        {
            return null;
        }

        _journeyPathVersionMapper.updateEntity(journeyPathVersionInput, existingJourneyPathVersion);
        await _context.SaveChangesAsync();
        return _journeyPathVersionMapper.toOutput(existingJourneyPathVersion);
    }

    public async Task<bool> DeleteJourneyPathVersionAsync(Guid journeyPathVersionId)
    {
        var journeyPathVersion = await _context.JourneyPathVersions.FindAsync(journeyPathVersionId);
        if (journeyPathVersion == null)
        {
            return false;
        }

        _context.JourneyPathVersions.Remove(journeyPathVersion);
        await _context.SaveChangesAsync();
        return true;
    }
}
