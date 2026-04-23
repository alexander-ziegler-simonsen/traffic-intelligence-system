using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("simulator_configs")]
public class SimulatorConfig
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("entity_type")]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    [Column("entity_id")]
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;

    [Column("route_id")]
    public Guid RouteId { get; set; }

    [Column("tick_interval_ms")]
    public int TickIntervalMs { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
