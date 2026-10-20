using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Services.Interfaces;

public interface IUsingJourneyLogService
{
    public Task<UsingJourneyLogOutput?> GetUsingJourneyLogByIdAsync(Guid usingJourneyLogId);
    public Task<List<UsingJourneyLogOutput>> GetAllUsingJourneyLogsAsync();
    public Task<UsingJourneyLogOutput> CreateUsingJourneyLogAsync(UsingJourneyLogInput usingJourneyLogInput);
    public Task<UsingJourneyLogOutput?> UpdateUsingJourneyLogAsync(Guid usingJourneyLogId, UsingJourneyLogInput usingJourneyLogInput);
    public Task<bool> DeleteUsingJourneyLogAsync(Guid usingJourneyLogId);
}
