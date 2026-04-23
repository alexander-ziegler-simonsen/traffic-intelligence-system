using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("train_stations")]
public class TrainStation
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_route_id")]
    public Guid FkRouteId { get; set; }

    [Column("station_name")]
    [MaxLength(100)]
    public string StationName { get; set; } = string.Empty;

    [Column("osm_station_id")]
    [MaxLength(100)]
    public string OsmStationId { get; set; } = string.Empty;

    [Column("sequence_order")]
    public int SequenceOrder { get; set; }

    [Column("lat")]
    public double Lat { get; set; }

    [Column("lng")]
    public double Lng { get; set; }

    [Column("has_platform")]
    public bool HasPlatform { get; set; }

    [Column("platform_number")]
    [MaxLength(20)]
    public string? PlatformNumber { get; set; }

    public TrainRoute TrainRoute { get; set; } = null!;
    public ICollection<TrainJourneyStopEvent> JourneyStopEvents { get; set; } = [];
}
