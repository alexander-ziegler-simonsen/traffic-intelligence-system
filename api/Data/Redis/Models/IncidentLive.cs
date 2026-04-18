namespace TisApi.Data.Redis.Models;

public class IncidentLive
{
    public int IncidentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Severity { get; set; }
}
