using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;

namespace api.Services.Interfaces;

public interface IDisruptionService
{
    public Task<DisruptionOutput?> GetDisruptionByIdAsync(Guid disruptionId);
    public Task<List<DisruptionOutput>> GetAllDisruptionsAsync();
    public Task<DisruptionOutput> CreateDisruptionAsync(DisruptionInput disruptionInput);
    public Task<DisruptionOutput?> UpdateDisruptionAsync(Guid disruptionId, DisruptionInput disruptionInput);
    public Task<bool> DeleteDisruptionAsync(Guid disruptionId);
}
