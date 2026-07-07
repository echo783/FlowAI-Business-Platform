namespace FlowAI.Api.DTOs;

public class CreateApprovalRequestRequest
{
    public string RequestTitle { get; set; } = string.Empty;

    public string RequesterName { get; set; } = string.Empty;

    public string ApproverName { get; set; } = string.Empty;

    public string? Comment { get; set; }
}
