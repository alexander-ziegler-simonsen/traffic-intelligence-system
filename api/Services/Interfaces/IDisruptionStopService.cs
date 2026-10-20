using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Services.Interfaces;

public interface IDisruptionStopService
{
    public Task<DisruptionStopOutput?> GetDisruptionStopByIdAsync(Guid disruptionStopId);
    public Task<List<DisruptionStopOutput>> GetAllDisruptionStopsAsync();
    public Task<DisruptionStopOutput> CreateDisruptionStopAsync(DisruptionStopInput disruptionStopInput);
    public Task<DisruptionStopOutput?> UpdateDisruptionStopAsync(Guid disruptionStopId, DisruptionStopInput disruptionStopInput);
    public Task<bool> DeleteDisruptionStopAsync(Guid disruptionStopId);
}
