namespace TisApi.DTOs;

public record CameraLiveDto(int CameraId, int VehicleCount, int AvgSpeedKmh, long LastUpdated);

public record IncidentLiveDto(int IncidentId, string Status, string Type, int Severity);
