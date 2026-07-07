using FlowAI.Api.DTOs;
using FlowAI.Api.Models;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

/// <summary>
/// Manages contract registration and approval workflow entry points.
/// </summary>
[ApiController]
[Route("api/contracts")]
public class ContractsController : ControllerBase
{
    private readonly ContractService _contractService;
    private readonly ApprovalRequestService _approvalRequestService;

    public ContractsController(
        ContractService contractService,
        ApprovalRequestService approvalRequestService)
    {
        _contractService = contractService;
        _approvalRequestService = approvalRequestService;
    }

    /// <summary>
    /// Gets all contracts in the current in-memory workflow store.
    /// </summary>
    [HttpGet]
    public ActionResult<List<Contract>> GetAll()
    {
        var contracts = _contractService.GetAll();

        return Ok(contracts);
    }

    /// <summary>
    /// Gets a single contract by id.
    /// </summary>
    [HttpGet("{id:int}")]
    public ActionResult<Contract> GetById(int id)
    {
        var contract = _contractService.GetById(id);

        if (contract is null)
        {
            return NotFound();
        }

        return Ok(contract);
    }

    /// <summary>
    /// Registers a new contract in the initial registered status.
    /// </summary>
    [HttpPost]
    public ActionResult<Contract> Create(CreateContractRequest request)
    {
        var contract = _contractService.Create(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = contract.Id },
            contract
        );
    }

    /// <summary>
    /// Requests approval for a registered contract.
    /// </summary>
    [HttpPost("{id:int}/request-approval")]
    public ActionResult<ApprovalRequest> RequestApproval(int id, CreateApprovalRequestRequest request)
    {
        var result = _approvalRequestService.RequestContractApproval(id, request);

        if (result.IsNotFound)
        {
            return NotFound(result.Error);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.ApprovalRequest);
    }

    /// <summary>
    /// Approves a contract approval request and auto-creates a work order.
    /// </summary>
    [HttpPost("{id:int}/approve")]
    public ActionResult<Contract> Approve(int id)
    {
        var result = _approvalRequestService.ApproveContract(id);

        if (result.IsNotFound)
        {
            return NotFound(result.Error);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Contract);
    }

    /// <summary>
    /// Rejects a contract approval request with a comment.
    /// </summary>
    [HttpPost("{id:int}/reject")]
    public ActionResult<Contract> Reject(int id, RejectApprovalRequest request)
    {
        var result = _approvalRequestService.RejectContract(id, request);

        if (result.IsNotFound)
        {
            return NotFound(result.Error);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Contract);
    }

    /// <summary>
    /// Gets status history records through the legacy contract route.
    /// </summary>
    [HttpGet("histories")]
    public ActionResult<List<StatusHistory>> GetHistories()
    {
        var histories = _contractService.GetHistories();

        return Ok(histories);
    }
}
