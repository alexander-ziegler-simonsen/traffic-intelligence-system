using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("roads")]
public class Road
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("type")]
    [MaxLength(20)]
    public string Type { get; set; } = string.Empty;

    [Column("speed_limit")]
    public short SpeedLimit { get; set; }

    [Column("city")]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    public ICollection<Camera> Cameras { get; set; } = [];
}
