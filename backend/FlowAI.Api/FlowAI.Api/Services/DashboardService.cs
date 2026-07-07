using FlowAI.Api.DTOs;
using FlowAI.Api.Models;

namespace FlowAI.Api.Services;

public class DashboardService
{
    private readonly ContractService _contractService;
    private readonly WorkOrderService _workOrderService;
    private readonly SettlementService _settlementService;

    public DashboardService(
        ContractService contractService,
        WorkOrderService workOrderService,
        SettlementService settlementService)
    {
        _contractService = contractService;
        _workOrderService = workOrderService;
        _settlementService = settlementService;
    }

    public DashboardSummaryResponse GetSummary()
    {
        var contracts = _contractService.GetAll();
        var workOrders = _workOrderService.GetAll();
        var settlements = _settlementService.GetAll();

        return new DashboardSummaryResponse
        {
            TotalContracts = contracts.Count,
            ApprovedContracts = contracts.Count(x =>
                x.Status == BusinessStatus.ContractApproved ||
                x.Status == BusinessStatus.ContractConvertedToWork),
            TotalWorkOrders = workOrders.Count,
            WorkInProgress = workOrders.Count(x => x.Status == BusinessStatus.WorkInProgress),
            WorkCompleted = workOrders.Count(x => x.Status == BusinessStatus.WorkCompleted),
            TotalSettlements = settlements.Count,
            SettlementRequested = settlements.Count(x => x.Status == BusinessStatus.SettlementRequested),
            SettlementApproved = settlements.Count(x => x.Status == BusinessStatus.SettlementApproved),
            SettlementHeld = settlements.Count(x => x.Status == BusinessStatus.SettlementHeld)
        };
    }
}
