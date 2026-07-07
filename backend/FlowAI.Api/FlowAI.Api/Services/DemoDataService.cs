using FlowAI.Api.DTOs;

namespace FlowAI.Api.Services;

public class DemoDataService
{
    private readonly ContractService _contractService;
    private readonly ApprovalRequestService _approvalRequestService;
    private readonly WorkOrderService _workOrderService;
    private readonly SettlementService _settlementService;
    private readonly StatusHistoryService _statusHistoryService;
    private readonly DashboardService _dashboardService;

    public DemoDataService(
        ContractService contractService,
        ApprovalRequestService approvalRequestService,
        WorkOrderService workOrderService,
        SettlementService settlementService,
        StatusHistoryService statusHistoryService,
        DashboardService dashboardService)
    {
        _contractService = contractService;
        _approvalRequestService = approvalRequestService;
        _workOrderService = workOrderService;
        _settlementService = settlementService;
        _statusHistoryService = statusHistoryService;
        _dashboardService = dashboardService;
    }

    public object Reset()
    {
        ClearAll();

        CreatePendingApprovalContract(
            contractNo: "CT-DEMO-0001",
            customerName: "Sample Customer",
            title: "Warehouse Workflow Automation",
            amount: 12000000);

        CreatePendingApprovalContract(
            contractNo: "CT-DEMO-0002",
            customerName: "Hanbit Logistics",
            title: "Logistics Maintenance Contract",
            amount: 8000000);

        CreateRejectedContract(
            contractNo: "CT-DEMO-0004",
            customerName: "Dongwoo Construction",
            title: "Construction Site Support Contract",
            amount: 6000000);

        var convertedContract = _contractService.Create(new CreateContractRequest
        {
            ContractNo = "CT-DEMO-0003",
            CustomerName = "Sejin Industry",
            Title = "ERP Modernization Pilot",
            Amount = 15000000
        });

        _approvalRequestService.RequestContractApproval(
            convertedContract.Id,
            new CreateApprovalRequestRequest
            {
                RequestTitle = "Approve ERP modernization pilot",
                RequesterName = "Demo Manager",
                ApproverName = "Operations Lead",
                Comment = "Demo contract for work and settlement flow."
            });

        var approvalResult = _approvalRequestService.ApproveContract(convertedContract.Id);
        var completedWorkOrder = approvalResult.WorkOrder!;

        _workOrderService.Start(completedWorkOrder.Id);
        _workOrderService.Complete(completedWorkOrder.Id);

        var inProgressWorkOrder = _workOrderService.Create(new CreateWorkOrderRequest
        {
            ContractId = convertedContract.Id,
            WorkNo = "WO-DEMO-0002",
            Title = "Follow-up integration work"
        }).WorkOrder!;

        _workOrderService.Start(inProgressWorkOrder.Id);

        CreateSettlementRequested(completedWorkOrder.Id, "ST-DEMO-0001", 3000000);
        CreateSettlementReviewing(completedWorkOrder.Id, "ST-DEMO-0002", 2500000);
        CreateSettlementApproved(completedWorkOrder.Id, "ST-DEMO-0003", 4500000);
        CreateSettlementHeld(completedWorkOrder.Id, "ST-DEMO-0004", 1800000);
        CreateSettlementRejected(completedWorkOrder.Id, "ST-DEMO-0005", 1200000);

        // TODO: Move this demo seed to a database-backed seeding strategy when persistence is introduced.
        return new
        {
            Message = "Demo data reset completed.",
            Dashboard = _dashboardService.GetSummary()
        };
    }

    private void ClearAll()
    {
        _settlementService.Clear();
        _workOrderService.Clear();
        _approvalRequestService.Clear();
        _contractService.Clear();
        _statusHistoryService.Clear();
    }

    private void CreatePendingApprovalContract(
        string contractNo,
        string customerName,
        string title,
        decimal amount)
    {
        var contract = _contractService.Create(new CreateContractRequest
        {
            ContractNo = contractNo,
            CustomerName = customerName,
            Title = title,
            Amount = amount
        });

        _approvalRequestService.RequestContractApproval(
            contract.Id,
            new CreateApprovalRequestRequest
            {
                RequestTitle = $"Approve {title}",
                RequesterName = "Demo Manager",
                ApproverName = "Operations Lead",
                Comment = "Seeded approval request."
            });
    }

    private void CreateRejectedContract(
        string contractNo,
        string customerName,
        string title,
        decimal amount)
    {
        var contract = _contractService.Create(new CreateContractRequest
        {
            ContractNo = contractNo,
            CustomerName = customerName,
            Title = title,
            Amount = amount
        });

        _approvalRequestService.RequestContractApproval(
            contract.Id,
            new CreateApprovalRequestRequest
            {
                RequestTitle = $"Review {title}",
                RequesterName = "Demo Manager",
                ApproverName = "Operations Lead",
                Comment = "Seeded approval request for rejection example."
            });

        _approvalRequestService.RejectContract(
            contract.Id,
            new RejectApprovalRequest
            {
                Comment = "Demo rejection for portfolio status coverage."
            });
    }

    private void CreateSettlementRequested(int workOrderId, string settlementNo, decimal amount)
    {
        _settlementService.Create(new CreateSettlementRequest
        {
            WorkOrderId = workOrderId,
            SettlementNo = settlementNo,
            Amount = amount
        });
    }

    private int CreateSettlementReviewing(int workOrderId, string settlementNo, decimal amount)
    {
        var settlement = _settlementService.Create(new CreateSettlementRequest
        {
            WorkOrderId = workOrderId,
            SettlementNo = settlementNo,
            Amount = amount
        }).Settlement!;

        _settlementService.Review(settlement.Id);

        return settlement.Id;
    }

    private void CreateSettlementApproved(int workOrderId, string settlementNo, decimal amount)
    {
        var settlementId = CreateSettlementReviewing(workOrderId, settlementNo, amount);
        _settlementService.Approve(settlementId);
    }

    private void CreateSettlementHeld(int workOrderId, string settlementNo, decimal amount)
    {
        var settlementId = CreateSettlementReviewing(workOrderId, settlementNo, amount);
        _settlementService.Hold(settlementId, new HoldSettlementRequest
        {
            HoldReason = "Amount verification required"
        });
    }

    private void CreateSettlementRejected(int workOrderId, string settlementNo, decimal amount)
    {
        var settlementId = CreateSettlementReviewing(workOrderId, settlementNo, amount);
        _settlementService.Reject(settlementId, new RejectSettlementRequest
        {
            RejectReason = "Supporting document is missing."
        });
    }
}
