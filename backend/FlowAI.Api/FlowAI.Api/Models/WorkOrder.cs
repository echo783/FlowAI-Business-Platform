namespace FlowAI.Api.Models;

public class WorkOrder
{
    public int Id { get; set; }

    public int ContractId { get; set; }

    public string WorkNo { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public BusinessStatus Status { get; set; } = BusinessStatus.WorkCreated;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}