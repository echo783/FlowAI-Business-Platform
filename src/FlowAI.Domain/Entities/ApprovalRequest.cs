using FlowAI.Domain.Enums;

namespace FlowAI.Domain.Entities;

public class ApprovalRequest
{
    public int Id { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public string RequestTitle { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Requested;
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string? Comment { get; set; }
}
