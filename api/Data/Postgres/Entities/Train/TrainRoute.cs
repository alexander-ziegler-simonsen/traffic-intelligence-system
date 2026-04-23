using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("train_routes")]
public class TrainRoute
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("line_id")]
    [MaxLength(50)]
    public string LineId { get; set; } = string.Empty;

    [Column("line_name")]
    [MaxLength(100)]
    public string LineName { get; set; } = string.Empty;

    [Column("direction")]
    [MaxLength(50)]
    public string Direction { get; set; } = string.Empty;

    [Column("train_type")]
    [MaxLength(50)]
    public string TrainType { get; set; } = string.Empty;

    [Column("capacity_per_train")]
    public int CapacityPerTrain { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<TrainStation> Stations { get; set; } = [];
    public ICollection<TrainRouteAssignment> RouteAssignments { get; set; } = [];
    public ICollection<TrainJourney> Journeys { get; set; } = [];
}
