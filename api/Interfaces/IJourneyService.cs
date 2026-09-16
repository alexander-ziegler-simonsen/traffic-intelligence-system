using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Interfaces;

public interface IJourneyService
{
    public Task<JourneyOutput?> GetJourneyByIdAsync(Guid journeyId);
    public Task<List<JourneyOutput>> GetAllJourneysAsync();
    public Task<JourneyOutput> CreateJourneyAsync(JourneyInput journeyInput);
    public Task<JourneyOutput?> UpdateJourneyAsync(Guid journeyId, JourneyInput journeyInput);
    public Task<bool> DeleteJourneyAsync(Guid journeyId);
}
