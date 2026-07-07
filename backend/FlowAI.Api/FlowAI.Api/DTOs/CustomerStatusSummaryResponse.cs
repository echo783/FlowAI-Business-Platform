namespace FlowAI.Api.DTOs;

public class CustomerStatusSummaryResponse
{
    public string CustomerName { get; set; } = string.Empty;

    public int ContractCount { get; set; }

    public int ApprovedContractCount { get; set; }

    public int WorkOrderCount { get; set; }

    public int WorkInProgressCount { get; set; }

    public int WorkCompletedCount { get; set; }

    public int SettlementCount { get; set; }

    public int SettlementApprovedCount { get; set; }

    public int SettlementHeldCount { get; set; }

    public string Summary { get; set; } = string.Empty;
}
