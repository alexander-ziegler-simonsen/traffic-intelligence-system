using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("traffic_light_phases")]
public class TrafficLightPhase
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_traffic_light_id")]
    public Guid FkTrafficLightId { get; set; }

    [Column("phase_name")]
    [MaxLength(100)]
    public string PhaseName { get; set; } = string.Empty;

    [Column("direction")]
    [MaxLength(50)]
    public string Direction { get; set; } = string.Empty;

    [Column("normal_green_seconds")]
    public int NormalGreenSeconds { get; set; }

    [Column("sequence_order")]
    public int SequenceOrder { get; set; }

    public TrafficLight TrafficLight { get; set; } = null!;
    public ICollection<TrafficLightOverrideEvent> OverrideEvents { get; set; } = [];
}
