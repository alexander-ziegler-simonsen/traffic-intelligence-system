using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TisApi.Data.Postgres.Entities;

[Table("subscribers")]
public class Subscriber
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("webhook_url")]
    [MaxLength(500)]
    public string WebhookUrl { get; set; } = string.Empty;

    [Column("secret_key")]
    [MaxLength(255)]
    public string SecretKey { get; set; } = string.Empty;

    [Column("line_ids", TypeName = "varchar[]")]
    public string[] LineIds { get; set; } = [];

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<WebhookDelivery> WebhookDeliveries { get; set; } = [];
}
