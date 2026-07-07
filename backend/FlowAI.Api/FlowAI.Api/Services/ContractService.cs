using FlowAI.Api.DTOs;
using FlowAI.Api.Models;

namespace FlowAI.Api.Services;

public class ContractService
{
    private readonly StatusHistoryService _statusHistoryService;
    private readonly List<Contract> _contracts = new();

    private int _contractSeq = 1;

    public ContractService(StatusHistoryService statusHistoryService)
    {
        _statusHistoryService = statusHistoryService;
    }

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

        _statusHistoryService.AddHistory(
            entityType: "Contract",
            entityId: contract.Id,
            fromStatus: BusinessStatus.ContractRegistered,
            toStatus: BusinessStatus.ContractRegistered,
            memo: "계약 등록"
        );

        return contract;
    }

    public Contract? UpdateStatus(
        int id,
        BusinessStatus toStatus,
        string memo,
        bool setApprovedAt = false)
    {
        var contract = GetById(id);

        if (contract is null)
        {
            return null;
        }

        var fromStatus = contract.Status;
        contract.Status = toStatus;

        if (setApprovedAt)
        {
            contract.ApprovedAt = DateTime.Now;
        }

        _statusHistoryService.AddHistory(
            entityType: "Contract",
            entityId: contract.Id,
            fromStatus: fromStatus,
            toStatus: toStatus,
            memo: memo
        );

        return contract;
    }

    public List<StatusHistory> GetHistories()
    {
        return _statusHistoryService.GetAll();
    }

    public void Clear()
    {
        _contracts.Clear();
        _contractSeq = 1;
    }
}
