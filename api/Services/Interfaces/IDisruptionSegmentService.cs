using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Services.Interfaces;

public interface IDisruptionSegmentService
{
    public Task<DisruptionSegmentOutput?> GetDisruptionSegmentByIdAsync(Guid disruptionSegmentId);
    public Task<List<DisruptionSegmentOutput>> GetAllDisruptionSegmentsAsync();
    public Task<DisruptionSegmentOutput> CreateDisruptionSegmentAsync(DisruptionSegmentInput disruptionSegmentInput);
    public Task<DisruptionSegmentOutput?> UpdateDisruptionSegmentAsync(Guid disruptionSegmentId, DisruptionSegmentInput disruptionSegmentInput);
    public Task<bool> DeleteDisruptionSegmentAsync(Guid disruptionSegmentId);
}
