using FlowAI.Application.Common;
using FlowAI.Domain.Entities;
using FlowAI.Domain.Enums;

namespace FlowAI.Application.Agent;

public sealed record CustomerStatusSummary(string CustomerName, int Contracts, int WorkOrders, int Settlements, decimal TotalAmount);

public sealed class AgentService(IFlowAiRepository repository)
{
    public IReadOnlyList<Contract> GetPendingApprovalContracts() =>
        repository.Contracts.Where(x => x.Status == ContractStatus.ApprovalRequested).ToList();

    public IReadOnlyList<WorkOrder> GetTodayWorkOrders()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return repository.WorkOrders
            .Where(x => x.Status == WorkOrderStatus.InProgress && x.PlannedStartDate <= today && x.PlannedEndDate >= today)
            .ToList();
    }

    public IReadOnlyList<WorkOrder> GetDelayedWorkOrders()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return repository.WorkOrders
            .Where(x => x.Status != WorkOrderStatus.Completed && x.PlannedEndDate < today)
            .ToList();
    }

    public IReadOnlyList<Settlement> GetOnHoldSettlements() =>
        repository.Settlements.Where(x => x.Status == SettlementStatus.OnHold).ToList();

    public CustomerStatusSummary GetCustomerStatusSummary(string customerName)
    {
        var contracts = repository.Contracts.Where(x => x.CustomerName == customerName).ToList();
        var contractIds = contracts.Select(x => x.Id).ToHashSet();
        return new CustomerStatusSummary(
            customerName,
            contracts.Count,
            repository.WorkOrders.Count(x => contractIds.Contains(x.ContractId)),
            repository.Settlements.Count(x => contractIds.Contains(x.ContractId)),
            contracts.Sum(x => x.Amount));
    }
}
