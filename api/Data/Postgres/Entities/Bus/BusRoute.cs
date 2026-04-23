using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("bus_routes")]
public class BusRoute
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

    [Column("capacity_per_bus")]
    public int CapacityPerBus { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<BusStop> BusStops { get; set; } = [];
    public ICollection<BusRouteAssignment> RouteAssignments { get; set; } = [];
    public ICollection<BusJourney> Journeys { get; set; } = [];
}
