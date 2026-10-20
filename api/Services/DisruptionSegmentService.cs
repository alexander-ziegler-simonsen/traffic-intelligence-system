using Microsoft.EntityFrameworkCore;
using api.data;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;
using api.Services.Interfaces;

namespace api.Services;

public class DisruptionSegmentService : IDisruptionSegmentService
{
    private readonly TisDbContext _context;
    private readonly DisruptionSegmentMapper _disruptionSegmentMapper;

    public DisruptionSegmentService(TisDbContext context, DisruptionSegmentMapper disruptionSegmentMapper)
    {
        _context = context;
        _disruptionSegmentMapper = disruptionSegmentMapper;
    }

    public async Task<DisruptionSegmentOutput?> GetDisruptionSegmentByIdAsync(Guid disruptionSegmentId)
    {
        var disruptionSegment = await _context.DisruptionSegments.FindAsync(disruptionSegmentId);
        return disruptionSegment != null ? _disruptionSegmentMapper.toOutput(disruptionSegment) : null;
    }

    public async Task<List<DisruptionSegmentOutput>> GetAllDisruptionSegmentsAsync()
    {
        var disruptionSegments = await _context.DisruptionSegments.ToListAsync();
        return disruptionSegments.Select(_disruptionSegmentMapper.toOutput).ToList();
    }

    public async Task<DisruptionSegmentOutput> CreateDisruptionSegmentAsync(DisruptionSegmentInput disruptionSegmentInput)
    {
        var disruptionSegment = _disruptionSegmentMapper.toEntity(disruptionSegmentInput);
        _context.DisruptionSegments.Add(disruptionSegment);
        await _context.SaveChangesAsync();
        return _disruptionSegmentMapper.toOutput(disruptionSegment);
    }

    public async Task<DisruptionSegmentOutput?> UpdateDisruptionSegmentAsync(Guid disruptionSegmentId, DisruptionSegmentInput disruptionSegmentInput)
    {
        var existingDisruptionSegment = await _context.DisruptionSegments.FindAsync(disruptionSegmentId);
        if (existingDisruptionSegment == null)
        {
            return null;
        }

        _disruptionSegmentMapper.updateEntity(disruptionSegmentInput, existingDisruptionSegment);
        await _context.SaveChangesAsync();
        return _disruptionSegmentMapper.toOutput(existingDisruptionSegment);
    }

    public async Task<bool> DeleteDisruptionSegmentAsync(Guid disruptionSegmentId)
    {
        var disruptionSegment = await _context.DisruptionSegments.FindAsync(disruptionSegmentId);
        if (disruptionSegment == null)
        {
            return false;
        }

        _context.DisruptionSegments.Remove(disruptionSegment);
        await _context.SaveChangesAsync();
        return true;
    }
}
