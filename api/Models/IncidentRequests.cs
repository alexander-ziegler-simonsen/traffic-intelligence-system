using System.ComponentModel.DataAnnotations;

namespace TisApi.Models;

public record CreateIncidentRequest(
    int CameraId,
    [Required][RegularExpression("accident|congestion|roadwork|hazard")] string Type,
    [Range(1, 5)] short Severity,
    DateTimeOffset? RecordedAt = null
);
