namespace FlowAI.Api.Models;

public class StatusHistory
{
    public int Id { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    public BusinessStatus FromStatus { get; set; }

    public BusinessStatus ToStatus { get; set; }

    public string ChangedBy { get; set; } = "system";

    public string? Memo { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.Now;
}