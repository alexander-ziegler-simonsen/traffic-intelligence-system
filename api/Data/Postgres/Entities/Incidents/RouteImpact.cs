using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("route_impacts")]
public class RouteImpact
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_incident_id")]
    public Guid FkIncidentId { get; set; }

    [Column("line_id")]
    [MaxLength(50)]
    public string LineId { get; set; } = string.Empty;

    [Column("line_name")]
    [MaxLength(100)]
    public string LineName { get; set; } = string.Empty;

    [Column("impact_level")]
    [MaxLength(20)]
    public string ImpactLevel { get; set; } = string.Empty;

    [Column("detected_at")]
    public DateTimeOffset DetectedAt { get; set; }

    public Incident Incident { get; set; } = null!;
    public ICollection<RerouteDecision> RerouteDecisions { get; set; } = [];
}
