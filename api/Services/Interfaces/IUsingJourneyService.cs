using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Services.Interfaces;

public interface IUsingJourneyService
{
    public Task<UsingJourneyOutput?> GetUsingJourneyByIdAsync(Guid usingJourneyId);
    public Task<List<UsingJourneyOutput>> GetAllUsingJourneysAsync();
    public Task<UsingJourneyOutput> CreateUsingJourneyAsync(UsingJourneyInput usingJourneyInput);
    public Task<UsingJourneyOutput?> UpdateUsingJourneyAsync(Guid usingJourneyId, UsingJourneyInput usingJourneyInput);
    public Task<bool> DeleteUsingJourneyAsync(Guid usingJourneyId);
}
