namespace FlowAI.Api.DTOs;

public class DashboardSummaryResponse
{
    public int TotalContracts { get; set; }

    public int ApprovedContracts { get; set; }

    public int TotalWorkOrders { get; set; }

    public int WorkInProgress { get; set; }

    public int WorkCompleted { get; set; }

    public int TotalSettlements { get; set; }

    public int SettlementRequested { get; set; }

    public int SettlementReviewing { get; set; }

    public int SettlementApproved { get; set; }

    public int SettlementHeld { get; set; }

    public int SettlementRejected { get; set; }
}
