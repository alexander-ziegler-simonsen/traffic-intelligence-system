using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("webhook_deliveries")]
public class WebhookDelivery
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("fk_incident_id")]
    public Guid? FkIncidentId { get; set; }

    [Column("fk_subscriber_id")]
    public Guid FkSubscriberId { get; set; }

    [Column("event_type")]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    [Column("payload", TypeName = "jsonb")]
    public string Payload { get; set; } = string.Empty;

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    [Column("attempt_number")]
    public int AttemptNumber { get; set; }

    [Column("http_status_code")]
    public int? HttpStatusCode { get; set; }

    [Column("error_message")]
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    [Column("attempted_at")]
    public DateTimeOffset? AttemptedAt { get; set; }

    [Column("next_retry_at")]
    public DateTimeOffset? NextRetryAt { get; set; }

    public Incident? Incident { get; set; }
    public Subscriber Subscriber { get; set; } = null!;
}
