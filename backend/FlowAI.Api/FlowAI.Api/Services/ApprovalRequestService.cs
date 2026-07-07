using FlowAI.Api.DTOs;
using FlowAI.Api.Models;

namespace FlowAI.Api.Services;

public class ApprovalRequestService
{
    private readonly ContractService _contractService;
    private readonly WorkOrderService _workOrderService;
    private readonly StatusHistoryService _statusHistoryService;
    private readonly List<ApprovalRequest> _approvalRequests = new();

    private int _approvalRequestSeq = 1;

    public ApprovalRequestService(
        ContractService contractService,
        WorkOrderService workOrderService,
        StatusHistoryService statusHistoryService)
    {
        _contractService = contractService;
        _workOrderService = workOrderService;
        _statusHistoryService = statusHistoryService;
    }

    public List<ApprovalRequest> GetAll()
    {
        return _approvalRequests
            .OrderByDescending(x => x.RequestedAt)
            .ToList();
    }

    public ApprovalRequest? GetById(int id)
    {
        return _approvalRequests.FirstOrDefault(x => x.Id == id);
    }

    public void Clear()
    {
        _approvalRequests.Clear();
        _approvalRequestSeq = 1;
    }

    public ApprovalWorkflowResult RequestContractApproval(int contractId, CreateApprovalRequestRequest request)
    {
        var contract = _contractService.GetById(contractId);

        if (contract is null)
        {
            return ApprovalWorkflowResult.NotFound("Contract not found.");
        }

        if (contract.Status != BusinessStatus.ContractRegistered)
        {
            return ApprovalWorkflowResult.Invalid("Only registered contracts can request approval.");
        }

        if (FindActiveContractApproval(contractId) is not null)
        {
            return ApprovalWorkflowResult.Invalid("This contract already has an active approval request.");
        }

        var approvalRequest = new ApprovalRequest
        {
            Id = _approvalRequestSeq++,
            TargetType = "Contract",
            TargetId = contractId,
            RequestTitle = request.RequestTitle,
            RequesterName = request.RequesterName,
            ApproverName = request.ApproverName,
            Status = BusinessStatus.ApprovalRequested,
            RequestedAt = DateTime.Now,
            Comment = request.Comment
        };

        _approvalRequests.Add(approvalRequest);

        _contractService.UpdateStatus(
            contractId,
            BusinessStatus.ContractApprovalRequested,
            "Contract approval requested"
        );

        _statusHistoryService.AddHistory(
            entityType: "ApprovalRequest",
            entityId: approvalRequest.Id,
            fromStatus: BusinessStatus.ApprovalRequested,
            toStatus: BusinessStatus.ApprovalRequested,
            memo: "Approval request created"
        );

        return ApprovalWorkflowResult.Success(contract, approvalRequest);
    }

    public ApprovalWorkflowResult ApproveContract(int contractId)
    {
        var contract = _contractService.GetById(contractId);

        if (contract is null)
        {
            return ApprovalWorkflowResult.NotFound("Contract not found.");
        }

        if (contract.Status != BusinessStatus.ContractApprovalRequested)
        {
            return ApprovalWorkflowResult.Invalid("Only approval-requested contracts can be approved.");
        }

        var approvalRequest = FindActiveContractApproval(contractId);

        if (approvalRequest is null)
        {
            return ApprovalWorkflowResult.Invalid("Approval request not found.");
        }

        var approvalBeforeStatus = approvalRequest.Status;
        approvalRequest.Status = BusinessStatus.ApprovalApproved;
        approvalRequest.ApprovedAt = DateTime.Now;

        _statusHistoryService.AddHistory(
            entityType: "ApprovalRequest",
            entityId: approvalRequest.Id,
            fromStatus: approvalBeforeStatus,
            toStatus: BusinessStatus.ApprovalApproved,
            memo: "Approval request approved"
        );

        var approvedContract = _contractService.UpdateStatus(
            contractId,
            BusinessStatus.ContractApproved,
            "Contract approved",
            setApprovedAt: true
        )!;

        var workOrder = _workOrderService.CreateAutomaticForContract(approvedContract);

        var convertedContract = _contractService.UpdateStatus(
            contractId,
            BusinessStatus.ContractConvertedToWork,
            "Contract converted to work order"
        )!;

        return ApprovalWorkflowResult.Success(convertedContract, approvalRequest, workOrder);
    }

    public ApprovalWorkflowResult RejectContract(int contractId, RejectApprovalRequest request)
    {
        var contract = _contractService.GetById(contractId);

        if (contract is null)
        {
            return ApprovalWorkflowResult.NotFound("Contract not found.");
        }

        if (contract.Status != BusinessStatus.ContractApprovalRequested)
        {
            return ApprovalWorkflowResult.Invalid("Only approval-requested contracts can be rejected.");
        }

        var approvalRequest = FindActiveContractApproval(contractId);

        if (approvalRequest is null)
        {
            return ApprovalWorkflowResult.Invalid("Approval request not found.");
        }

        var approvalBeforeStatus = approvalRequest.Status;
        approvalRequest.Status = BusinessStatus.ApprovalRejected;
        approvalRequest.RejectedAt = DateTime.Now;
        approvalRequest.Comment = request.Comment;

        _statusHistoryService.AddHistory(
            entityType: "ApprovalRequest",
            entityId: approvalRequest.Id,
            fromStatus: approvalBeforeStatus,
            toStatus: BusinessStatus.ApprovalRejected,
            memo: $"Approval request rejected: {request.Comment}"
        );

        var rejectedContract = _contractService.UpdateStatus(
            contractId,
            BusinessStatus.ContractRejected,
            $"Contract rejected: {request.Comment}"
        )!;

        return ApprovalWorkflowResult.Success(rejectedContract, approvalRequest);
    }

    private ApprovalRequest? FindActiveContractApproval(int contractId)
    {
        return _approvalRequests.FirstOrDefault(x =>
            x.TargetType == "Contract" &&
            x.TargetId == contractId &&
            x.Status == BusinessStatus.ApprovalRequested);
    }
}

public class ApprovalWorkflowResult
{
    private ApprovalWorkflowResult(
        Contract? contract,
        ApprovalRequest? approvalRequest,
        WorkOrder? workOrder,
        string? error,
        bool isNotFound)
    {
        Contract = contract;
        ApprovalRequest = approvalRequest;
        WorkOrder = workOrder;
        Error = error;
        IsNotFound = isNotFound;
    }

    public Contract? Contract { get; }

    public ApprovalRequest? ApprovalRequest { get; }

    public WorkOrder? WorkOrder { get; }

    public string? Error { get; }

    public bool IsNotFound { get; }

    public bool IsSuccess => Contract is not null || ApprovalRequest is not null || WorkOrder is not null;

    public static ApprovalWorkflowResult Success(
        Contract contract,
        ApprovalRequest approvalRequest,
        WorkOrder? workOrder = null)
    {
        return new ApprovalWorkflowResult(contract, approvalRequest, workOrder, null, false);
    }

    public static ApprovalWorkflowResult NotFound(string error)
    {
        return new ApprovalWorkflowResult(null, null, null, error, true);
    }

    public static ApprovalWorkflowResult Invalid(string error)
    {
        return new ApprovalWorkflowResult(null, null, null, error, false);
    }
}
