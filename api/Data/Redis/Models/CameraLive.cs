namespace TisApi.Data.Redis.Models;

public class CameraLive
{
    public int CameraId { get; set; }
    public int VehicleCount { get; set; }
    public int AvgSpeedKmh { get; set; }
    public long LastUpdated { get; set; }
}
