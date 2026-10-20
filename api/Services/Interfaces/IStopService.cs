using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Services.Interfaces;

public interface IStopService
{
    public Task<StopOutput?> GetStopByIdAsync(Guid stopId);
    public Task<List<StopOutput>> GetAllStopsAsync();
    public Task<StopOutput> CreateStopAsync(StopInput stopInput);
    public Task<StopOutput?> UpdateStopAsync(Guid stopId, StopInput stopInput);
    public Task<bool> DeleteStopAsync(Guid stopId);
}
