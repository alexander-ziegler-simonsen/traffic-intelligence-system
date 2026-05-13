using TisApi.DTOs;
using TisApi.Models;

namespace TisApi.Services.Interfaces;

public interface IIncidentService
{
    Task<IReadOnlyList<IncidentDto>> GetAllAsync(string? type = null, string? status = null);
    Task<IncidentDto?> GetByIdAsync(Guid id);
    Task<IncidentDto> CreateAsync(CreateIncidentRequest request);
    Task<bool> DeleteAsync(Guid id);
}
