using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Services.Interfaces;

public interface IJourneyPathVersionService
{
    public Task<JourneyPathVersionOutput?> GetJourneyPathVersionByIdAsync(Guid journeyPathVersionId);
    public Task<List<JourneyPathVersionOutput>> GetAllJourneyPathVersionsAsync();
    public Task<JourneyPathVersionOutput> CreateJourneyPathVersionAsync(JourneyPathVersionInput journeyPathVersionInput);
    public Task<JourneyPathVersionOutput?> UpdateJourneyPathVersionAsync(Guid journeyPathVersionId, JourneyPathVersionInput journeyPathVersionInput);
    public Task<bool> DeleteJourneyPathVersionAsync(Guid journeyPathVersionId);
}
