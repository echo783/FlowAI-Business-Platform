namespace FlowAI.Api.Models;

public class Contract
{
    public int Id { get; set; }

    public string ContractNo { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public BusinessStatus Status { get; set; } = BusinessStatus.ContractRegistered;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? ApprovedAt { get; set; }
}