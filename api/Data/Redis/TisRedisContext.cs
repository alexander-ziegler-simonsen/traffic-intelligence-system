using StackExchange.Redis;
using TisApi.Data.Redis.Models;

namespace TisApi.Data.Redis;

public class TisRedisContext(IConnectionMultiplexer redis)
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<CameraLive?> GetCameraLiveAsync(int cameraId)
    {
        var entries = await _db.HashGetAllAsync($"live:camera:{cameraId}");
        if (entries.Length == 0) return null;

        var map = entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());
        return new CameraLive
        {
            CameraId = cameraId,
            VehicleCount = int.TryParse(map.GetValueOrDefault("vehicle_count"), out var vc) ? vc : 0,
            AvgSpeedKmh = int.TryParse(map.GetValueOrDefault("avg_speed_kmh"), out var sp) ? sp : 0,
            LastUpdated = long.TryParse(map.GetValueOrDefault("last_updated"), out var lu) ? lu : 0,
        };
    }

    public async Task<IncidentLive?> GetIncidentLiveAsync(int incidentId)
    {
        var entries = await _db.HashGetAllAsync($"live:incident:{incidentId}");
        if (entries.Length == 0) return null;

        var map = entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());
        return new IncidentLive
        {
            IncidentId = incidentId,
            Status = map.GetValueOrDefault("status") ?? string.Empty,
            Type = map.GetValueOrDefault("type") ?? string.Empty,
            Severity = int.TryParse(map.GetValueOrDefault("severity"), out var sev) ? sev : 0,
        };
    }

    public async Task<IReadOnlyList<int>> GetRoadCameraIdsAsync(int roadId)
    {
        var members = await _db.SetMembersAsync($"live:road:{roadId}:cameras");
        return members
            .Select(m => int.TryParse(m.ToString(), out var id) ? id : -1)
            .Where(id => id > 0)
            .ToList();
    }
}
