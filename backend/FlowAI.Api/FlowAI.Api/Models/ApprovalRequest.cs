namespace FlowAI.Api.Models;

public class ApprovalRequest
{
    public int Id { get; set; }

    public string TargetType { get; set; } = string.Empty;

    public int TargetId { get; set; }

    public string RequestTitle { get; set; } = string.Empty;

    public string RequesterName { get; set; } = string.Empty;

    public string ApproverName { get; set; } = string.Empty;

    public BusinessStatus Status { get; set; } = BusinessStatus.ApprovalRequested;

    public DateTime RequestedAt { get; set; } = DateTime.Now;

    public DateTime? ApprovedAt { get; set; }

    public DateTime? RejectedAt { get; set; }

    public string? Comment { get; set; }
}
