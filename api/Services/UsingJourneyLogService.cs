using Microsoft.EntityFrameworkCore;
using api.data;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;
using api.Services.Interfaces;

namespace api.Services;

public class UsingJourneyLogService : IUsingJourneyLogService
{
    private readonly TisDbContext _context;
    private readonly UsingJourneyLogMapper _usingJourneyLogMapper;

    public UsingJourneyLogService(TisDbContext context, UsingJourneyLogMapper usingJourneyLogMapper)
    {
        _context = context;
        _usingJourneyLogMapper = usingJourneyLogMapper;
    }

    public async Task<UsingJourneyLogOutput?> GetUsingJourneyLogByIdAsync(Guid usingJourneyLogId)
    {
        var usingJourneyLog = await _context.UsingJourneyLogs.FindAsync(usingJourneyLogId);
        return usingJourneyLog != null ? _usingJourneyLogMapper.toOutput(usingJourneyLog) : null;
    }

    public async Task<List<UsingJourneyLogOutput>> GetAllUsingJourneyLogsAsync()
    {
        var usingJourneyLogs = await _context.UsingJourneyLogs.ToListAsync();
        return usingJourneyLogs.Select(_usingJourneyLogMapper.toOutput).ToList();
    }

    public async Task<UsingJourneyLogOutput> CreateUsingJourneyLogAsync(UsingJourneyLogInput usingJourneyLogInput)
    {
        var usingJourneyLog = _usingJourneyLogMapper.toEntity(usingJourneyLogInput);
        _context.UsingJourneyLogs.Add(usingJourneyLog);
        await _context.SaveChangesAsync();
        return _usingJourneyLogMapper.toOutput(usingJourneyLog);
    }

    public async Task<UsingJourneyLogOutput?> UpdateUsingJourneyLogAsync(Guid usingJourneyLogId, UsingJourneyLogInput usingJourneyLogInput)
    {
        var existingUsingJourneyLog = await _context.UsingJourneyLogs.FindAsync(usingJourneyLogId);
        if (existingUsingJourneyLog == null)
        {
            return null;
        }

        _usingJourneyLogMapper.updateEntity(usingJourneyLogInput, existingUsingJourneyLog);
        await _context.SaveChangesAsync();
        return _usingJourneyLogMapper.toOutput(existingUsingJourneyLog);
    }

    public async Task<bool> DeleteUsingJourneyLogAsync(Guid usingJourneyLogId)
    {
        var usingJourneyLog = await _context.UsingJourneyLogs.FindAsync(usingJourneyLogId);
        if (usingJourneyLog == null)
        {
            return false;
        }

        _context.UsingJourneyLogs.Remove(usingJourneyLog);
        await _context.SaveChangesAsync();
        return true;
    }
}
