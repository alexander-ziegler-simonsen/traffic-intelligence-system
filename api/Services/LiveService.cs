using TisApi.Data.Redis;
using TisApi.DTOs;
using TisApi.Mappers;
using TisApi.Services.Interfaces;

namespace TisApi.Services;

public class LiveService(TisRedisContext redis, LiveMapper mapper) : ILiveService
{
    public async Task<CameraLiveDto?> GetCameraAsync(int cameraId)
    {
        var data = await redis.GetCameraLiveAsync(cameraId);
        return data is null ? null : mapper.ToDto(data);
    }

    public async Task<IncidentLiveDto?> GetIncidentAsync(int incidentId)
    {
        var data = await redis.GetIncidentLiveAsync(incidentId);
        return data is null ? null : mapper.ToDto(data);
    }

    public Task<IReadOnlyList<int>> GetRoadCameraIdsAsync(int roadId) =>
        redis.GetRoadCameraIdsAsync(roadId);
}
