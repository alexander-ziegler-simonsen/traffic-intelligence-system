using TisApi.DTOs;
using TisApi.Models;

namespace TisApi.Services.Interfaces;

public interface IRoadService
{
    Task<IReadOnlyList<RoadDto>> GetAllAsync();
    Task<RoadDto?> GetByIdAsync(int id);
    Task<RoadDto> CreateAsync(CreateRoadRequest request);
    Task<RoadDto?> UpdateAsync(int id, UpdateRoadRequest request);
    Task<bool> DeleteAsync(int id);
}
