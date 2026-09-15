using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("traffic_lights")]
public class TrafficLight
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("osm_node_id")]
    [MaxLength(100)]
    public string OsmNodeId { get; set; } = string.Empty;

    [Column("lat")]
    public double Lat { get; set; }

    [Column("lng")]
    public double Lng { get; set; }

    [Column("road_segment_id")]
    [MaxLength(100)]
    public string RoadSegmentId { get; set; } = string.Empty;

    [Column("normal_green_seconds")]
    public int NormalGreenSeconds { get; set; }

    [Column("normal_red_seconds")]
    public int NormalRedSeconds { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<TrafficLightPhase> Phases { get; set; } = [];
    public ICollection<TrafficLightOverrideEvent> OverrideEvents { get; set; } = [];
}
