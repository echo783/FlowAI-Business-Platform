using FlowAI.Application.Common;
using FlowAI.Domain.Enums;

namespace FlowAI.Application.Dashboard;

public sealed record DashboardSummary(int PendingApprovalContracts, int InProgressWorkOrders, int CompletedWorkOrders, int RequestedSettlements, int OnHoldSettlements, int WorkOrdersDueToday);

public sealed class DashboardService(IFlowAiRepository repository)
{
    public DashboardSummary GetSummary()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return new DashboardSummary(
            repository.Contracts.Count(x => x.Status == ContractStatus.ApprovalRequested),
            repository.WorkOrders.Count(x => x.Status == WorkOrderStatus.InProgress),
            repository.WorkOrders.Count(x => x.Status == WorkOrderStatus.Completed),
            repository.Settlements.Count(x => x.Status == SettlementStatus.Requested),
            repository.Settlements.Count(x => x.Status == SettlementStatus.OnHold),
            repository.WorkOrders.Count(x => x.PlannedEndDate == today));
    }
}
