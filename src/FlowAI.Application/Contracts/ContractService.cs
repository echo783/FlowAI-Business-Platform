using FlowAI.Application.Common;
using FlowAI.Domain.Entities;
using FlowAI.Domain.Enums;

namespace FlowAI.Application.Contracts;

public sealed class ContractService(IFlowAiRepository repository)
{
    private readonly StatusHistoryWriter _history = new(repository);

    public IReadOnlyList<Contract> GetAll() => repository.Contracts.OrderByDescending(x => x.CreatedAt).ToList();

    public Contract GetById(int id) => repository.Contracts.FirstOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException("Contract was not found.");

    public async Task<Contract> CreateAsync(CreateContractRequest request, CancellationToken cancellationToken = default)
    {
        var contract = new Contract
        {
            ContractNo = request.ContractNo,
            Title = request.Title,
            CustomerName = request.CustomerName,
            Amount = request.Amount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ManagerName = request.ManagerName,
            Status = ContractStatus.Registered
        };

        repository.Add(contract);
        await repository.SaveChangesAsync(cancellationToken);
        _history.Add("Contract", contract.Id, ContractStatus.Draft, ContractStatus.Registered, request.ManagerName, "Contract registered");
        await repository.SaveChangesAsync(cancellationToken);
        return contract;
    }

    public async Task<Contract> UpdateAsync(int id, UpdateContractRequest request, CancellationToken cancellationToken = default)
    {
        var contract = GetById(id);
        contract.Title = request.Title;
        contract.CustomerName = request.CustomerName;
        contract.Amount = request.Amount;
        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;
        contract.ManagerName = request.ManagerName;
        contract.Status = request.Status;
        contract.UpdatedAt = DateTimeOffset.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return contract;
    }

    public async Task<ApprovalRequest> RequestApprovalAsync(int id, RequestApprovalRequest request, CancellationToken cancellationToken = default)
    {
        var contract = GetById(id);
        if (contract.Status != ContractStatus.Registered)
        {
            throw new BusinessRuleException("Only registered contracts can request approval.");
        }

        var previous = contract.Status;
        contract.Status = ContractStatus.ApprovalRequested;
        contract.UpdatedAt = DateTimeOffset.UtcNow;

        var approval = new ApprovalRequest
        {
            TargetType = "Contract",
            TargetId = contract.Id,
            RequestTitle = $"{contract.ContractNo} approval",
            RequesterName = request.RequesterName,
            ApproverName = request.ApproverName,
            Comment = request.Comment
        };

        repository.Add(approval);
        _history.Add("Contract", contract.Id, previous, contract.Status, request.RequesterName, request.Comment);
        await repository.SaveChangesAsync(cancellationToken);
        return approval;
    }

    public async Task<WorkOrder> ApproveAsync(int id, ApprovalDecisionRequest request, CancellationToken cancellationToken = default)
    {
        var contract = GetById(id);
        if (contract.Status != ContractStatus.ApprovalRequested)
        {
            throw new BusinessRuleException("Only approval-requested contracts can be approved.");
        }

        var approval = repository.ApprovalRequests
            .Where(x => x.TargetType == "Contract" && x.TargetId == id && x.Status == ApprovalStatus.Requested)
            .OrderByDescending(x => x.RequestedAt)
            .FirstOrDefault();

        if (approval is not null)
        {
            approval.Status = ApprovalStatus.Approved;
            approval.ApprovedAt = DateTimeOffset.UtcNow;
            approval.Comment = request.Comment;
        }

        var approvedFrom = contract.Status;
        contract.Status = ContractStatus.Approved;
        contract.UpdatedAt = DateTimeOffset.UtcNow;
        _history.Add("Contract", contract.Id, approvedFrom, contract.Status, request.ChangedBy, request.Comment);

        var workOrder = new WorkOrder
        {
            ContractId = contract.Id,
            WorkNo = $"WO-{DateTime.UtcNow:yyyyMMdd}-{contract.Id:0000}",
            Title = $"{contract.Title} work order",
            AssignedTo = contract.ManagerName,
            Status = WorkOrderStatus.Created,
            PlannedStartDate = contract.StartDate,
            PlannedEndDate = contract.EndDate
        };
        repository.Add(workOrder);
        await repository.SaveChangesAsync(cancellationToken);

        var convertedFrom = contract.Status;
        contract.Status = ContractStatus.ConvertedToWork;
        contract.UpdatedAt = DateTimeOffset.UtcNow;
        _history.Add("Contract", contract.Id, convertedFrom, contract.Status, request.ChangedBy, "Work order created");
        await repository.SaveChangesAsync(cancellationToken);
        return workOrder;
    }

    public async Task<Contract> RejectAsync(int id, ApprovalDecisionRequest request, CancellationToken cancellationToken = default)
    {
        var contract = GetById(id);
        if (contract.Status != ContractStatus.ApprovalRequested)
        {
            throw new BusinessRuleException("Only approval-requested contracts can be rejected.");
        }

        var previous = contract.Status;
        contract.Status = ContractStatus.Rejected;
        contract.UpdatedAt = DateTimeOffset.UtcNow;
        _history.Add("Contract", contract.Id, previous, contract.Status, request.ChangedBy, request.Comment);
        await repository.SaveChangesAsync(cancellationToken);
        return contract;
    }
}
