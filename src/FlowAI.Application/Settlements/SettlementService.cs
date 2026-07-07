using FlowAI.Application.Common;
using FlowAI.Domain.Entities;
using FlowAI.Domain.Enums;

namespace FlowAI.Application.Settlements;

public sealed record SettlementActionRequest(string ChangedBy, string? Reason);
public sealed record SettlementHoldRequest(string ChangedBy, string HoldReason);

public sealed class SettlementService(IFlowAiRepository repository)
{
    private readonly StatusHistoryWriter _history = new(repository);

    public IReadOnlyList<Settlement> GetAll() => repository.Settlements.OrderByDescending(x => x.CreatedAt).ToList();

    public Settlement GetById(int id) => repository.Settlements.FirstOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException("Settlement was not found.");

    public async Task<Settlement> RequestForWorkOrderAsync(int workOrderId, SettlementActionRequest request, CancellationToken cancellationToken = default)
    {
        var workOrder = repository.WorkOrders.FirstOrDefault(x => x.Id == workOrderId)
            ?? throw new KeyNotFoundException("Work order was not found.");
        if (workOrder.Status != WorkOrderStatus.Completed)
        {
            throw new BusinessRuleException("Only completed work orders can request settlement.");
        }

        var contract = repository.Contracts.First(x => x.Id == workOrder.ContractId);
        var settlement = new Settlement
        {
            ContractId = contract.Id,
            WorkOrderId = workOrder.Id,
            SettlementNo = $"ST-{DateTime.UtcNow:yyyyMMdd}-{workOrder.Id:0000}",
            Amount = contract.Amount,
            Status = SettlementStatus.Requested
        };

        repository.Add(settlement);
        await repository.SaveChangesAsync(cancellationToken);
        _history.Add("Settlement", settlement.Id, SettlementStatus.Requested, SettlementStatus.Requested, request.ChangedBy, request.Reason);
        await repository.SaveChangesAsync(cancellationToken);
        return settlement;
    }

    public Task<Settlement> ReviewAsync(int id, SettlementActionRequest request, CancellationToken cancellationToken = default)
    {
        var settlement = GetById(id);
        if (settlement.Status != SettlementStatus.Requested)
        {
            throw new BusinessRuleException("Only requested settlements can move to reviewing.");
        }

        settlement.ReviewedAt = DateTimeOffset.UtcNow;
        return ChangeStatusAsync(settlement, SettlementStatus.Reviewing, request.ChangedBy, request.Reason, cancellationToken);
    }

    public Task<Settlement> ApproveAsync(int id, SettlementActionRequest request, CancellationToken cancellationToken = default)
    {
        var settlement = GetById(id);
        if (settlement.Status != SettlementStatus.Reviewing)
        {
            throw new BusinessRuleException("Only reviewing settlements can be approved.");
        }

        settlement.ApprovedAt = DateTimeOffset.UtcNow;
        return ChangeStatusAsync(settlement, SettlementStatus.Approved, request.ChangedBy, request.Reason, cancellationToken);
    }

    public Task<Settlement> HoldAsync(int id, SettlementHoldRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.HoldReason))
        {
            throw new BusinessRuleException("Hold reason is required.");
        }

        var settlement = GetById(id);
        if (settlement.Status != SettlementStatus.Reviewing)
        {
            throw new BusinessRuleException("Only reviewing settlements can be put on hold.");
        }

        settlement.HoldReason = request.HoldReason;
        return ChangeStatusAsync(settlement, SettlementStatus.OnHold, request.ChangedBy, request.HoldReason, cancellationToken);
    }

    public Task<Settlement> RejectAsync(int id, SettlementActionRequest request, CancellationToken cancellationToken = default)
    {
        var settlement = GetById(id);
        if (settlement.Status != SettlementStatus.Reviewing)
        {
            throw new BusinessRuleException("Only reviewing settlements can be rejected.");
        }

        return ChangeStatusAsync(settlement, SettlementStatus.Rejected, request.ChangedBy, request.Reason, cancellationToken);
    }

    private async Task<Settlement> ChangeStatusAsync(Settlement settlement, SettlementStatus toStatus, string changedBy, string? reason, CancellationToken cancellationToken)
    {
        if (settlement.Status == SettlementStatus.Approved)
        {
            throw new BusinessRuleException("Approved settlements cannot move back to hold or rejection.");
        }

        var previous = settlement.Status;
        settlement.Status = toStatus;
        settlement.UpdatedAt = DateTimeOffset.UtcNow;
        _history.Add("Settlement", settlement.Id, previous, toStatus, changedBy, reason);
        await repository.SaveChangesAsync(cancellationToken);
        return settlement;
    }
}
