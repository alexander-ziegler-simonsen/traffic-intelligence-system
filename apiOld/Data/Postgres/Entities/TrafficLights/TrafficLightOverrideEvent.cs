using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("traffic_light_override_events")]
public class TrafficLightOverrideEvent
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_traffic_light_id")]
    public Guid FkTrafficLightId { get; set; }

    [Column("fk_phase_id")]
    public Guid FkPhaseId { get; set; }

    [Column("trigger_type")]
    [MaxLength(50)]
    public string TriggerType { get; set; } = string.Empty;

    [Column("trigger_detail", TypeName = "jsonb")]
    public string TriggerDetail { get; set; } = string.Empty;

    [Column("override_duration_seconds")]
    public int OverrideDurationSeconds { get; set; }

    [Column("started_at")]
    public DateTimeOffset StartedAt { get; set; }

    [Column("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }

    [Column("returned_to_normal_at")]
    public DateTimeOffset? ReturnedToNormalAt { get; set; }

    public TrafficLight TrafficLight { get; set; } = null!;
    public TrafficLightPhase Phase { get; set; } = null!;
}
