using FlowAI.Api.DTOs;
using FlowAI.Api.Models;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

[ApiController]
[Route("api/contracts")]
public class ContractsController : ControllerBase
{
    private readonly ContractService _contractService;

    public ContractsController(ContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    public ActionResult<List<Contract>> GetAll()
    {
        var contracts = _contractService.GetAll();

        return Ok(contracts);
    }

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

    [HttpPost("{id:int}/approve")]
    public ActionResult<Contract> Approve(int id)
    {
        var contract = _contractService.Approve(id);

        if (contract is null)
        {
            return NotFound();
        }

        return Ok(contract);
    }

    [HttpGet("histories")]
    public ActionResult<List<StatusHistory>> GetHistories()
    {
        var histories = _contractService.GetHistories();

        return Ok(histories);
    }
}