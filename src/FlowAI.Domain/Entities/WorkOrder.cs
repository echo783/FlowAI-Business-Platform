using FlowAI.Domain.Enums;

namespace FlowAI.Domain.Entities;

public class WorkOrder
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string WorkNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Created;
    public DateOnly PlannedStartDate { get; set; }
    public DateOnly PlannedEndDate { get; set; }
    public DateOnly? ActualStartDate { get; set; }
    public DateOnly? ActualEndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
