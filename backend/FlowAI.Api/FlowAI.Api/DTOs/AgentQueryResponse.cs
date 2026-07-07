namespace FlowAI.Api.DTOs;

public class AgentQueryResponse
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public string Source { get; set; } = "rule-based";

    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    public AgentQueryFactsResponse? Facts { get; set; }
}

public class AgentQueryFactsResponse
{
    public int PendingApprovalContracts { get; set; }

    public int TodayWorkOrders { get; set; }

    public int DelayedWorkOrders { get; set; }

    public int OnHoldSettlements { get; set; }

    public int TotalSettlements { get; set; }

    public int RequestedSettlements { get; set; }

    public int ReviewingSettlements { get; set; }

    public int ApprovedSettlements { get; set; }

    public int RejectedSettlements { get; set; }
}
