using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("cameras")]
public class Camera
{
    [Column("id")]
    public int Id { get; set; }

    [Column("road_id")]
    public int RoadId { get; set; }

    [Column("label")]
    [MaxLength(20)]
    public string Label { get; set; } = string.Empty;

    [Column("latitude")]
    public decimal Latitude { get; set; }

    [Column("longitude")]
    public decimal Longitude { get; set; }

    [Column("status")]
    [MaxLength(10)]
    public string Status { get; set; } = "active";

    public Road Road { get; set; } = null!;
    public ICollection<Incident> Incidents { get; set; } = [];
}
