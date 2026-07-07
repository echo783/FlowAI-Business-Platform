using FlowAI.Domain.Enums;

namespace FlowAI.Domain.Entities;

public class Settlement
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int WorkOrderId { get; set; }
    public string SettlementNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public SettlementStatus Status { get; set; } = SettlementStatus.Requested;
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? HoldReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
