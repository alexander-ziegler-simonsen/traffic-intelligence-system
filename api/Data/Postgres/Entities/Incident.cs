using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("incidents")]
public class Incident
{
    [Column("id")]
    public int Id { get; set; }

    [Column("camera_id")]
    public int CameraId { get; set; }

    [Column("type")]
    [MaxLength(20)]
    public string Type { get; set; } = string.Empty;

    [Column("severity")]
    public short Severity { get; set; }

    [Column("recorded_at")]
    public DateTimeOffset RecordedAt { get; set; }

    public Camera Camera { get; set; } = null!;
}
