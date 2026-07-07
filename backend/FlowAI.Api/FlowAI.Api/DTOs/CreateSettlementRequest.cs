namespace FlowAI.Api.DTOs;

public class CreateSettlementRequest
{
    public int WorkOrderId { get; set; }

    public string SettlementNo { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
