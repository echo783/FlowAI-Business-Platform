using FlowAI.Api.DTOs;
using FlowAI.Api.Models;

namespace FlowAI.Api.Services;

public class ContractService
{
    private readonly List<Contract> _contracts = new();
    private readonly List<StatusHistory> _histories = new();

    private int _contractSeq = 1;
    private int _historySeq = 1;

    public List<Contract> GetAll()
    {
        return _contracts
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public Contract? GetById(int id)
    {
        return _contracts.FirstOrDefault(x => x.Id == id);
    }

    public Contract Create(CreateContractRequest request)
    {
        var contract = new Contract
        {
            Id = _contractSeq++,
            ContractNo = request.ContractNo,
            CustomerName = request.CustomerName,
            Title = request.Title,
            Amount = request.Amount,
            Status = BusinessStatus.ContractRegistered,
            CreatedAt = DateTime.Now
        };

        _contracts.Add(contract);

        AddHistory(
            entityType: "Contract",
            entityId: contract.Id,
            fromStatus: BusinessStatus.ContractRegistered,
            toStatus: BusinessStatus.ContractRegistered,
            memo: "계약 등록"
        );

        return contract;
    }

    public Contract? Approve(int id)
    {
        var contract = GetById(id);

        if (contract is null)
        {
            return null;
        }

        var beforeStatus = contract.Status;

        if (beforeStatus == BusinessStatus.ContractApproved)
        {
            return contract;
        }

        contract.Status = BusinessStatus.ContractApproved;
        contract.ApprovedAt = DateTime.Now;

        AddHistory(
            entityType: "Contract",
            entityId: contract.Id,
            fromStatus: beforeStatus,
            toStatus: BusinessStatus.ContractApproved,
            memo: "계약 승인"
        );

        return contract;
    }

    public List<StatusHistory> GetHistories()
    {
        return _histories
            .OrderByDescending(x => x.ChangedAt)
            .ToList();
    }

    private void AddHistory(
        string entityType,
        int entityId,
        BusinessStatus fromStatus,
        BusinessStatus toStatus,
        string memo)
    {
        var history = new StatusHistory
        {
            Id = _historySeq++,
            EntityType = entityType,
            EntityId = entityId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedBy = "system",
            Memo = memo,
            ChangedAt = DateTime.Now
        };

        _histories.Add(history);
    }
}