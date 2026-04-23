using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("bus_journeys")]
public class BusJourney
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_route_id")]
    public Guid FkRouteId { get; set; }

    [Column("fk_route_assignment_id")]
    public Guid FkRouteAssignmentId { get; set; }

    [Column("bus_identifier")]
    [MaxLength(100)]
    public string BusIdentifier { get; set; } = string.Empty;

    [Column("started_at")]
    public DateTimeOffset StartedAt { get; set; }

    [Column("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    public BusRoute BusRoute { get; set; } = null!;
    public BusRouteAssignment RouteAssignment { get; set; } = null!;
    public ICollection<BusJourneyStopEvent> StopEvents { get; set; } = [];
}
