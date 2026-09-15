using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("train_journeys")]
public class TrainJourney
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_route_id")]
    public Guid FkRouteId { get; set; }

    [Column("fk_route_assignment_id")]
    public Guid FkRouteAssignmentId { get; set; }

    [Column("train_identifier")]
    [MaxLength(100)]
    public string TrainIdentifier { get; set; } = string.Empty;

    [Column("started_at")]
    public DateTimeOffset StartedAt { get; set; }

    [Column("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    [Column("delay_minutes")]
    public int DelayMinutes { get; set; }

    public TrainRoute TrainRoute { get; set; } = null!;
    public TrainRouteAssignment RouteAssignment { get; set; } = null!;
    public ICollection<TrainJourneyStopEvent> StopEvents { get; set; } = [];
}
