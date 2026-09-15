namespace TisApi.DTOs;

public record IncidentReportDto(
    string Id,
    int PostgresId,
    string Type,
    int Severity,
    DateTime RecordedAt,
    CameraInfoDto Camera,
    RoadInfoDto Road
);

public record CameraInfoDto(int Id, string Label, double Latitude, double Longitude, string Status);

public record RoadInfoDto(int Id, string Name, string Type, int SpeedLimit, string City);
