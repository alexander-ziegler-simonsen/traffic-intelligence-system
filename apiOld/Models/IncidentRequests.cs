using System.ComponentModel.DataAnnotations;

namespace TisApi.Models;

public record CreateIncidentRequest(
    [Required][RegularExpression("accident|congestion|roadwork|hazard")] string Type,
    [Required] string Status,
    double Lat,
    double Lng,
    [Required] string RoadSegmentId,
    string Description = "",
    DateTimeOffset? ReportedAt = null
);
