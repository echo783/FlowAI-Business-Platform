using FlowAI.Domain.Enums;

namespace FlowAI.Application.Contracts;

public sealed record CreateContractRequest(string ContractNo, string Title, string CustomerName, decimal Amount, DateOnly StartDate, DateOnly EndDate, string ManagerName);
public sealed record UpdateContractRequest(string Title, string CustomerName, decimal Amount, DateOnly StartDate, DateOnly EndDate, string ManagerName, ContractStatus Status);
public sealed record RequestApprovalRequest(string RequesterName, string ApproverName, string? Comment);
public sealed record ApprovalDecisionRequest(string ChangedBy, string? Comment);
