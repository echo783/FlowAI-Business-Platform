namespace FlowAI.Api.Models;

public class Settlement
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }

    public string SettlementNo { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public BusinessStatus Status { get; set; } = BusinessStatus.SettlementRequested;

    public string? HoldReason { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.Now;

    public DateTime? ApprovedAt { get; set; }
}