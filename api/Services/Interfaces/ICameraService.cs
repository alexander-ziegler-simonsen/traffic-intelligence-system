using TisApi.DTOs;
using TisApi.Models;

namespace TisApi.Services.Interfaces;

public interface ICameraService
{
    Task<IReadOnlyList<CameraDto>> GetAllAsync(int? roadId = null);
    Task<CameraDto?> GetByIdAsync(int id);
    Task<CameraDto> CreateAsync(CreateCameraRequest request);
    Task<CameraDto?> UpdateAsync(int id, UpdateCameraRequest request);
    Task<bool> DeleteAsync(int id);
}
