using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("bus_stops")]
public class BusStop
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_route_id")]
    public Guid FkRouteId { get; set; }

    [Column("stop_name")]
    [MaxLength(100)]
    public string StopName { get; set; } = string.Empty;

    [Column("osm_stop_id")]
    [MaxLength(100)]
    public string OsmStopId { get; set; } = string.Empty;

    [Column("sequence_order")]
    public int SequenceOrder { get; set; }

    [Column("lat")]
    public double Lat { get; set; }

    [Column("lng")]
    public double Lng { get; set; }

    public BusRoute BusRoute { get; set; } = null!;
    public ICollection<BusJourneyStopEvent> JourneyStopEvents { get; set; } = [];
}
