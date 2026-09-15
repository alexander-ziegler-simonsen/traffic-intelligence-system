using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("reroute_decisions")]
public class RerouteDecision
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_route_impact_id")]
    public Guid FkRouteImpactId { get; set; }

    [Column("detour_geometry", TypeName = "jsonb")]
    public string DetourGeometry { get; set; } = string.Empty;

    [Column("original_segment_id")]
    [MaxLength(100)]
    public string OriginalSegmentId { get; set; } = string.Empty;

    [Column("detour_via")]
    [MaxLength(255)]
    public string DetourVia { get; set; } = string.Empty;

    [Column("decided_at")]
    public DateTimeOffset DecidedAt { get; set; }

    [Column("revoked_at")]
    public DateTimeOffset? RevokedAt { get; set; }

    public RouteImpact RouteImpact { get; set; } = null!;
}
