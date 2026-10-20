using api.Models;
using api.Dtos.Output;

namespace api.Services.Interfaces;

public interface IDiversionService
{
    public Task<List<DiversionOutput>> GetDiversionsAsync();
    public Task<List<DiversionOutput>?> RerouteAsync(Guid disruptionId);
    public Task<List<JourneyPathVersion>> FindAffectedBusPathsAsync(IEnumerable<Guid> blockedStopIds, IEnumerable<(Guid From, Guid To)> blockedSegments, DateTime validFrom, DateTime? validTo);
    public Task RemoveDiversionsAsync(Guid disruptionId);
}
