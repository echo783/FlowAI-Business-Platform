using FlowAI.Application.Common;
using FlowAI.Domain.Entities;
using FlowAI.Domain.Enums;

namespace FlowAI.Application.WorkOrders;

public sealed record WorkOrderActionRequest(string ChangedBy, string? Reason);

public sealed class WorkOrderService(IFlowAiRepository repository)
{
    private readonly StatusHistoryWriter _history = new(repository);

    public IReadOnlyList<WorkOrder> GetAll() => repository.WorkOrders.OrderByDescending(x => x.CreatedAt).ToList();

    public WorkOrder GetById(int id) => repository.WorkOrders.FirstOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException("Work order was not found.");

    public Task<WorkOrder> StartAsync(int id, WorkOrderActionRequest request, CancellationToken cancellationToken = default)
    {
        var workOrder = GetById(id);
        if (workOrder.Status != WorkOrderStatus.Created)
        {
            throw new BusinessRuleException("Only created work orders can start.");
        }

        workOrder.ActualStartDate = DateOnly.FromDateTime(DateTime.Today);
        return ChangeStatusAsync(workOrder, WorkOrderStatus.InProgress, request, cancellationToken);
    }

    public Task<WorkOrder> CompleteAsync(int id, WorkOrderActionRequest request, CancellationToken cancellationToken = default)
    {
        var workOrder = GetById(id);
        if (workOrder.Status != WorkOrderStatus.InProgress)
        {
            throw new BusinessRuleException("Only in-progress work orders can complete.");
        }

        workOrder.ActualEndDate = DateOnly.FromDateTime(DateTime.Today);
        return ChangeStatusAsync(workOrder, WorkOrderStatus.Completed, request, cancellationToken);
    }

    public Task<WorkOrder> HoldAsync(int id, WorkOrderActionRequest request, CancellationToken cancellationToken = default)
    {
        var workOrder = GetById(id);
        return ChangeStatusAsync(workOrder, WorkOrderStatus.OnHold, request, cancellationToken);
    }

    private async Task<WorkOrder> ChangeStatusAsync(WorkOrder workOrder, WorkOrderStatus toStatus, WorkOrderActionRequest request, CancellationToken cancellationToken)
    {
        var previous = workOrder.Status;
        workOrder.Status = toStatus;
        workOrder.UpdatedAt = DateTimeOffset.UtcNow;
        _history.Add("WorkOrder", workOrder.Id, previous, toStatus, request.ChangedBy, request.Reason);
        await repository.SaveChangesAsync(cancellationToken);
        return workOrder;
    }
}
