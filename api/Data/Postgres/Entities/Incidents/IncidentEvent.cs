using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("incident_events")]
public class IncidentEvent
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_incident_id")]
    public Guid FkIncidentId { get; set; }

    [Column("sequence_number")]
    public int SequenceNumber { get; set; }

    [Column("event_type")]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    [Column("payload", TypeName = "jsonb")]
    public string Payload { get; set; } = string.Empty;

    [Column("occurred_at")]
    public DateTimeOffset OccurredAt { get; set; }

    public Incident Incident { get; set; } = null!;
}
