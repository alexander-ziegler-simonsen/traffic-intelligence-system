using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("incidents")]
public class Incident
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("type")]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    [Column("lat")]
    public double Lat { get; set; }

    [Column("lng")]
    public double Lng { get; set; }

    [Column("road_segment_id")]
    [MaxLength(100)]
    public string RoadSegmentId { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Column("reported_at")]
    public DateTimeOffset ReportedAt { get; set; }

    [Column("resolved_at")]
    public DateTimeOffset? ResolvedAt { get; set; }

    public ICollection<IncidentEvent> IncidentEvents { get; set; } = [];
    public ICollection<RouteImpact> RouteImpacts { get; set; } = [];
    public ICollection<WebhookDelivery> WebhookDeliveries { get; set; } = [];
}
