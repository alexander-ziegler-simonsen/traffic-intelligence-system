namespace TisApi.Messaging.Contracts;

public record IncidentCreated(
    string Type,
    string Status,
    double Lat,
    double Lng,
    string RoadSegmentId,
    string Description = "",
    DateTimeOffset? ReportedAt = null
);
