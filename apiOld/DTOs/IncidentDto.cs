namespace TisApi.DTOs;

public record IncidentDto(
    Guid Id,
    string Type,
    string Status,
    double Lat,
    double Lng,
    string RoadSegmentId,
    DateTimeOffset ReportedAt,
    DateTimeOffset? ResolvedAt
);
