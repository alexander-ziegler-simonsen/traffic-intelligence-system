using TisApi.DTOs;

namespace TisApi.Services.Interfaces;

public interface ILiveService
{
    Task<CameraLiveDto?> GetCameraAsync(int cameraId);
    Task<IncidentLiveDto?> GetIncidentAsync(int incidentId);
    Task<IReadOnlyList<int>> GetRoadCameraIdsAsync(int roadId);
}
