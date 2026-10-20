using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Services.Interfaces;

public interface IJourneyPathTimePlanService
{
    public Task<JourneyPathTimePlanOutput?> GetJourneyPathTimePlanByIdAsync(Guid journeyPathTimePlanId);
    public Task<List<JourneyPathTimePlanOutput>> GetAllJourneyPathTimePlansAsync();
    public Task<JourneyPathTimePlanOutput> CreateJourneyPathTimePlanAsync(JourneyPathTimePlanInput journeyPathTimePlanInput);
    public Task<JourneyPathTimePlanOutput?> UpdateJourneyPathTimePlanAsync(Guid journeyPathTimePlanId, JourneyPathTimePlanInput journeyPathTimePlanInput);
    public Task<bool> DeleteJourneyPathTimePlanAsync(Guid journeyPathTimePlanId);
}
