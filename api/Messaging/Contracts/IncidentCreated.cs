namespace TisApi.Messaging.Contracts;

public record IncidentCreated(
    int CameraId,
    string Type,
    short Severity,
    DateTimeOffset RecordedAt
);
