namespace TisApi.DTOs;

public record IncidentDto(int Id, int CameraId, string Type, short Severity, DateTimeOffset RecordedAt);
