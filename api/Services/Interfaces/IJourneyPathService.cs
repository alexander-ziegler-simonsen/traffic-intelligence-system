using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Services.Interfaces;

public interface IJourneyPathService
{
    public Task<JourneyPathOutput?> GetJourneyPathByIdAsync(Guid journeyPathId);
    public Task<List<JourneyPathOutput>> GetAllJourneyPathsAsync();
    public Task<JourneyPathOutput> CreateJourneyPathAsync(JourneyPathInput journeyPathInput);
    public Task<JourneyPathOutput?> UpdateJourneyPathAsync(Guid journeyPathId, JourneyPathInput journeyPathInput);
    public Task<bool> DeleteJourneyPathAsync(Guid journeyPathId);
}
