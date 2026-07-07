using FlowAI.Api.DTOs;
using FlowAI.Api.Models;

namespace FlowAI.Api.Services;

public class WorkOrderService
{
    private readonly ContractService _contractService;
    private readonly StatusHistoryService _statusHistoryService;
    private readonly List<WorkOrder> _workOrders = new();

    private int _workOrderSeq = 1;

    public WorkOrderService(
        ContractService contractService,
        StatusHistoryService statusHistoryService)
    {
        _contractService = contractService;
        _statusHistoryService = statusHistoryService;
    }

    public List<WorkOrder> GetAll()
    {
        return _workOrders
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public WorkOrder? GetById(int id)
    {
        return _workOrders.FirstOrDefault(x => x.Id == id);
    }

    public void Clear()
    {
        _workOrders.Clear();
        _workOrderSeq = 1;
    }

    public WorkOrder CreateAutomaticForContract(Contract contract)
    {
        var workOrder = new WorkOrder
        {
            Id = _workOrderSeq++,
            ContractId = contract.Id,
            WorkNo = $"WO-AUTO-{contract.Id:0000}",
            Title = $"{contract.Title} Work Order",
            Status = BusinessStatus.WorkCreated,
            CreatedAt = DateTime.Now
        };

        _workOrders.Add(workOrder);

        _statusHistoryService.AddHistory(
            entityType: "WorkOrder",
            entityId: workOrder.Id,
            fromStatus: BusinessStatus.WorkCreated,
            toStatus: BusinessStatus.WorkCreated,
            memo: "Work order auto-created after contract approval"
        );

        return workOrder;
    }

    public WorkOrderCreateResult Create(CreateWorkOrderRequest request)
    {
        var contract = _contractService.GetById(request.ContractId);

        if (contract is null)
        {
            return WorkOrderCreateResult.NotFound("Contract not found.");
        }

        if (contract.Status != BusinessStatus.ContractApproved &&
            contract.Status != BusinessStatus.ContractConvertedToWork)
        {
            return WorkOrderCreateResult.Invalid("Only approved or work-converted contracts can create work orders.");
        }

        var workOrder = new WorkOrder
        {
            Id = _workOrderSeq++,
            ContractId = request.ContractId,
            WorkNo = request.WorkNo,
            Title = request.Title,
            Status = BusinessStatus.WorkCreated,
            CreatedAt = DateTime.Now
        };

        _workOrders.Add(workOrder);

        _statusHistoryService.AddHistory(
            entityType: "WorkOrder",
            entityId: workOrder.Id,
            fromStatus: BusinessStatus.WorkCreated,
            toStatus: BusinessStatus.WorkCreated,
            memo: "작업 생성"
        );

        return WorkOrderCreateResult.Success(workOrder);
    }

    public WorkOrderTransitionResult Start(int id)
    {
        var workOrder = GetById(id);

        if (workOrder is null)
        {
            return WorkOrderTransitionResult.NotFound("Work order not found.");
        }

        if (workOrder.Status != BusinessStatus.WorkCreated)
        {
            return WorkOrderTransitionResult.Invalid("Only created work orders can be started.");
        }

        var beforeStatus = workOrder.Status;

        workOrder.Status = BusinessStatus.WorkInProgress;
        workOrder.StartedAt = DateTime.Now;

        _statusHistoryService.AddHistory(
            entityType: "WorkOrder",
            entityId: workOrder.Id,
            fromStatus: beforeStatus,
            toStatus: BusinessStatus.WorkInProgress,
            memo: "작업 시작"
        );

        return WorkOrderTransitionResult.Success(workOrder);
    }

    public WorkOrderTransitionResult Complete(int id)
    {
        var workOrder = GetById(id);

        if (workOrder is null)
        {
            return WorkOrderTransitionResult.NotFound("Work order not found.");
        }

        if (workOrder.Status != BusinessStatus.WorkInProgress)
        {
            return WorkOrderTransitionResult.Invalid("Only in-progress work orders can be completed.");
        }

        var beforeStatus = workOrder.Status;

        workOrder.Status = BusinessStatus.WorkCompleted;
        workOrder.CompletedAt = DateTime.Now;

        _statusHistoryService.AddHistory(
            entityType: "WorkOrder",
            entityId: workOrder.Id,
            fromStatus: beforeStatus,
            toStatus: BusinessStatus.WorkCompleted,
            memo: "작업 완료"
        );

        return WorkOrderTransitionResult.Success(workOrder);
    }
}

public class WorkOrderCreateResult
{
    private WorkOrderCreateResult(WorkOrder? workOrder, string? error, bool isNotFound)
    {
        WorkOrder = workOrder;
        Error = error;
        IsNotFound = isNotFound;
    }

    public WorkOrder? WorkOrder { get; }

    public string? Error { get; }

    public bool IsNotFound { get; }

    public bool IsSuccess => WorkOrder is not null;

    public static WorkOrderCreateResult Success(WorkOrder workOrder)
    {
        return new WorkOrderCreateResult(workOrder, null, false);
    }

    public static WorkOrderCreateResult NotFound(string error)
    {
        return new WorkOrderCreateResult(null, error, true);
    }

    public static WorkOrderCreateResult Invalid(string error)
    {
        return new WorkOrderCreateResult(null, error, false);
    }
}

public class WorkOrderTransitionResult
{
    private WorkOrderTransitionResult(WorkOrder? workOrder, string? error, bool isNotFound)
    {
        WorkOrder = workOrder;
        Error = error;
        IsNotFound = isNotFound;
    }

    public WorkOrder? WorkOrder { get; }

    public string? Error { get; }

    public bool IsNotFound { get; }

    public bool IsSuccess => WorkOrder is not null;

    public static WorkOrderTransitionResult Success(WorkOrder workOrder)
    {
        return new WorkOrderTransitionResult(workOrder, null, false);
    }

    public static WorkOrderTransitionResult NotFound(string error)
    {
        return new WorkOrderTransitionResult(null, error, true);
    }

    public static WorkOrderTransitionResult Invalid(string error)
    {
        return new WorkOrderTransitionResult(null, error, false);
    }
}
