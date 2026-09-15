using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("train_route_assignments")]
public class TrainRouteAssignment
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_route_id")]
    public Guid FkRouteId { get; set; }

    [Column("train_identifier")]
    [MaxLength(100)]
    public string TrainIdentifier { get; set; } = string.Empty;

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    [Column("assigned_at")]
    public DateTimeOffset AssignedAt { get; set; }

    [Column("removed_at")]
    public DateTimeOffset? RemovedAt { get; set; }

    [Column("removal_reason")]
    [MaxLength(255)]
    public string? RemovalReason { get; set; }

    public TrainRoute TrainRoute { get; set; } = null!;
    public ICollection<TrainJourney> Journeys { get; set; } = [];
}
