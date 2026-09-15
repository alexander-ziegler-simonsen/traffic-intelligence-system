using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("bus_route_assignments")]
public class BusRouteAssignment
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_route_id")]
    public Guid FkRouteId { get; set; }

    [Column("bus_identifier")]
    [MaxLength(100)]
    public string BusIdentifier { get; set; } = string.Empty;

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

    public BusRoute BusRoute { get; set; } = null!;
    public ICollection<BusJourney> Journeys { get; set; } = [];
}
