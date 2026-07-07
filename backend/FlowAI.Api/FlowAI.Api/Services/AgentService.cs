using FlowAI.Api.DTOs;
using FlowAI.Api.Models;

namespace FlowAI.Api.Services;

public class AgentService
{
    private readonly ContractService _contractService;
    private readonly WorkOrderService _workOrderService;
    private readonly SettlementService _settlementService;
    private readonly DashboardService _dashboardService;

    public AgentService(
        ContractService contractService,
        WorkOrderService workOrderService,
        SettlementService settlementService,
        DashboardService dashboardService)
    {
        _contractService = contractService;
        _workOrderService = workOrderService;
        _settlementService = settlementService;
        _dashboardService = dashboardService;
    }

    public List<Contract> GetPendingApprovalContracts()
    {
        return _contractService.GetAll()
            .Where(x =>
                x.Status == BusinessStatus.ContractApprovalPending ||
                // TODO: Replace ContractRegistered with only ApprovalRequested/Pending after approval request workflow is added.
                x.Status == BusinessStatus.ContractRegistered)
            .ToList();
    }

    public List<WorkOrder> GetTodayWorkOrders()
    {
        var today = DateTime.Today;

        return _workOrderService.GetAll()
            .Where(x =>
                x.Status == BusinessStatus.WorkInProgress &&
                (!x.StartedAt.HasValue || x.StartedAt.Value.Date == today))
            .ToList();
    }

    public List<WorkOrder> GetDelayedWorkOrders()
    {
        var today = DateTime.Today;

        return _workOrderService.GetAll()
            .Where(x =>
                // TODO: Replace this temporary rule with PlannedEndDate based delay detection when the model has PlannedEndDate.
                (x.Status == BusinessStatus.WorkCreated || x.Status == BusinessStatus.WorkInProgress) &&
                x.CreatedAt.Date < today)
            .ToList();
    }

    public List<Settlement> GetOnHoldSettlements()
    {
        return _settlementService.GetAll()
            .Where(x => x.Status == BusinessStatus.SettlementHeld)
            .ToList();
    }

    public CustomerStatusSummaryResponse GetCustomerStatusSummary(string customerName)
    {
        var contracts = _contractService.GetAll()
            .Where(x => string.Equals(x.CustomerName, customerName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var contractIds = contracts.Select(x => x.Id).ToHashSet();
        var workOrders = _workOrderService.GetAll()
            .Where(x => contractIds.Contains(x.ContractId))
            .ToList();

        var workOrderIds = workOrders.Select(x => x.Id).ToHashSet();
        var settlements = _settlementService.GetAll()
            .Where(x => workOrderIds.Contains(x.WorkOrderId))
            .ToList();

        var response = new CustomerStatusSummaryResponse
        {
            CustomerName = customerName,
            ContractCount = contracts.Count,
            ApprovedContractCount = contracts.Count(x => x.Status == BusinessStatus.ContractApproved),
            WorkOrderCount = workOrders.Count,
            WorkInProgressCount = workOrders.Count(x => x.Status == BusinessStatus.WorkInProgress),
            WorkCompletedCount = workOrders.Count(x => x.Status == BusinessStatus.WorkCompleted),
            SettlementCount = settlements.Count,
            SettlementApprovedCount = settlements.Count(x => x.Status == BusinessStatus.SettlementApproved),
            SettlementHeldCount = settlements.Count(x => x.Status == BusinessStatus.SettlementHeld)
        };

        response.Summary =
            $"{customerName} has {response.ContractCount} contracts, " +
            $"{response.WorkOrderCount} work orders, and {response.SettlementCount} settlements. " +
            $"Approved contracts: {response.ApprovedContractCount}, completed work orders: {response.WorkCompletedCount}, " +
            $"approved settlements: {response.SettlementApprovedCount}, held settlements: {response.SettlementHeldCount}.";

        return response;
    }

    public AgentQueryResponse Query(AgentQueryRequest request)
    {
        var question = request.Question ?? string.Empty;
        var answer = BuildAnswer(question);

        return new AgentQueryResponse
        {
            Question = question,
            Answer = answer,
            Source = "mock",
            GeneratedAt = DateTime.Now
        };
    }

    private string BuildAnswer(string question)
    {
        if (ContainsAny(question, "정산 보류", "보류"))
        {
            var onHoldCount = GetOnHoldSettlements().Count;
            return $"현재 정산 보류 건은 {onHoldCount}건입니다.";
        }

        if (ContainsAny(question, "진행 중", "작업"))
        {
            var inProgressCount = _workOrderService.GetAll()
                .Count(x => x.Status == BusinessStatus.WorkInProgress);

            return $"현재 진행 중인 작업은 {inProgressCount}건입니다.";
        }

        if (ContainsAny(question, "승인 대기", "계약"))
        {
            var pendingCount = GetPendingApprovalContracts().Count;
            return $"현재 승인 대기 또는 승인 전 계약은 {pendingCount}건입니다.";
        }

        if (ContainsAny(question, "요약", "현황"))
        {
            var summary = _dashboardService.GetSummary();
            return
                $"전체 계약 {summary.TotalContracts}건, 승인 계약 {summary.ApprovedContracts}건, " +
                $"작업 {summary.TotalWorkOrders}건, 진행 중 작업 {summary.WorkInProgress}건, 완료 작업 {summary.WorkCompleted}건, " +
                $"정산 {summary.TotalSettlements}건, 정산 요청 {summary.SettlementRequested}건, " +
                $"정산 승인 {summary.SettlementApproved}건, 정산 보류 {summary.SettlementHeld}건입니다.";
        }

        return "현재는 계약, 작업, 정산, 대시보드 현황 조회를 지원합니다.";
    }

    private static bool ContainsAny(string value, params string[] keywords)
    {
        return keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
