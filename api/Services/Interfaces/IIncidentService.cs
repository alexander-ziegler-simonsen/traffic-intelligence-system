using TisApi.DTOs;
using TisApi.Models;

namespace TisApi.Services.Interfaces;

public interface IIncidentService
{
    Task<IReadOnlyList<IncidentDto>> GetAllAsync(int? cameraId = null, string? type = null, short? minSeverity = null);
    Task<IncidentDto?> GetByIdAsync(int id);
    Task<IncidentDto> CreateAsync(CreateIncidentRequest request);
    Task<bool> DeleteAsync(int id);
}
