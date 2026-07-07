using FlowAI.Api.DTOs;
using FlowAI.Api.Models;

namespace FlowAI.Api.Services;

public class SettlementService
{
    private readonly WorkOrderService _workOrderService;
    private readonly StatusHistoryService _statusHistoryService;
    private readonly List<Settlement> _settlements = new();

    private int _settlementSeq = 1;

    public SettlementService(
        WorkOrderService workOrderService,
        StatusHistoryService statusHistoryService)
    {
        _workOrderService = workOrderService;
        _statusHistoryService = statusHistoryService;
    }

    public List<Settlement> GetAll()
    {
        return _settlements
            .OrderByDescending(x => x.RequestedAt)
            .ToList();
    }

    public Settlement? GetById(int id)
    {
        return _settlements.FirstOrDefault(x => x.Id == id);
    }

    public SettlementResult Create(CreateSettlementRequest request)
    {
        var workOrder = _workOrderService.GetById(request.WorkOrderId);

        if (workOrder is null)
        {
            return SettlementResult.NotFound("Work order not found.");
        }

        if (workOrder.Status != BusinessStatus.WorkCompleted)
        {
            return SettlementResult.Invalid("Only completed work orders can request settlement.");
        }

        var settlement = new Settlement
        {
            Id = _settlementSeq++,
            WorkOrderId = request.WorkOrderId,
            SettlementNo = request.SettlementNo,
            Amount = request.Amount,
            Status = BusinessStatus.SettlementRequested,
            RequestedAt = DateTime.Now
        };

        _settlements.Add(settlement);

        _statusHistoryService.AddHistory(
            entityType: "Settlement",
            entityId: settlement.Id,
            fromStatus: BusinessStatus.SettlementRequested,
            toStatus: BusinessStatus.SettlementRequested,
            memo: "Settlement requested"
        );

        return SettlementResult.Success(settlement);
    }

    public SettlementResult Approve(int id)
    {
        var settlement = GetById(id);

        if (settlement is null)
        {
            return SettlementResult.NotFound("Settlement not found.");
        }

        if (settlement.Status != BusinessStatus.SettlementRequested)
        {
            return SettlementResult.Invalid("Only requested settlements can be approved.");
        }

        var beforeStatus = settlement.Status;

        settlement.Status = BusinessStatus.SettlementApproved;
        settlement.ApprovedAt = DateTime.Now;

        _statusHistoryService.AddHistory(
            entityType: "Settlement",
            entityId: settlement.Id,
            fromStatus: beforeStatus,
            toStatus: BusinessStatus.SettlementApproved,
            memo: "Settlement approved"
        );

        return SettlementResult.Success(settlement);
    }

    public SettlementResult Hold(int id, HoldSettlementRequest request)
    {
        var settlement = GetById(id);

        if (settlement is null)
        {
            return SettlementResult.NotFound("Settlement not found.");
        }

        if (settlement.Status != BusinessStatus.SettlementRequested)
        {
            return SettlementResult.Invalid("Only requested settlements can be held.");
        }

        var beforeStatus = settlement.Status;

        settlement.Status = BusinessStatus.SettlementHeld;
        settlement.HoldReason = request.HoldReason;

        _statusHistoryService.AddHistory(
            entityType: "Settlement",
            entityId: settlement.Id,
            fromStatus: beforeStatus,
            toStatus: BusinessStatus.SettlementHeld,
            memo: $"Settlement held: {request.HoldReason}"
        );

        return SettlementResult.Success(settlement);
    }
}

public class SettlementResult
{
    private SettlementResult(Settlement? settlement, string? error, bool isNotFound)
    {
        Settlement = settlement;
        Error = error;
        IsNotFound = isNotFound;
    }

    public Settlement? Settlement { get; }

    public string? Error { get; }

    public bool IsNotFound { get; }

    public bool IsSuccess => Settlement is not null;

    public static SettlementResult Success(Settlement settlement)
    {
        return new SettlementResult(settlement, null, false);
    }

    public static SettlementResult NotFound(string error)
    {
        return new SettlementResult(null, error, true);
    }

    public static SettlementResult Invalid(string error)
    {
        return new SettlementResult(null, error, false);
    }
}
