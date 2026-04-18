using TisApi.DTOs;

namespace TisApi.Services.Interfaces;

public interface IIncidentReportService
{
    Task<IReadOnlyList<IncidentReportDto>> GetAllAsync(int? minSeverity = null, string? roadName = null, string? cameraLabel = null);
    Task<IncidentReportDto?> GetByIdAsync(string id);
}
