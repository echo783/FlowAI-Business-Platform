using FlowAI.Api.DTOs;
using FlowAI.Api.Models;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

/// <summary>
/// Provides Agent query endpoints prepared for Copilot Studio and Power Platform connector scenarios.
/// </summary>
[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly AgentService _agentService;

    public AgentController(AgentService agentService)
    {
        _agentService = agentService;
    }

    /// <summary>
    /// Gets contracts that are waiting for approval or not yet approved.
    /// </summary>
    [HttpGet("contracts/pending-approval")]
    public ActionResult<List<Contract>> GetPendingApprovalContracts()
    {
        var contracts = _agentService.GetPendingApprovalContracts();

        return Ok(contracts);
    }

    /// <summary>
    /// Gets work orders currently in progress today.
    /// </summary>
    [HttpGet("workorders/today")]
    public ActionResult<List<WorkOrder>> GetTodayWorkOrders()
    {
        var workOrders = _agentService.GetTodayWorkOrders();

        return Ok(workOrders);
    }

    /// <summary>
    /// Gets delayed work order candidates using the current temporary delay rule.
    /// </summary>
    [HttpGet("workorders/delayed")]
    public ActionResult<List<WorkOrder>> GetDelayedWorkOrders()
    {
        var workOrders = _agentService.GetDelayedWorkOrders();

        return Ok(workOrders);
    }

    /// <summary>
    /// Gets settlements currently on hold.
    /// </summary>
    [HttpGet("settlements/on-hold")]
    public ActionResult<List<Settlement>> GetOnHoldSettlements()
    {
        var settlements = _agentService.GetOnHoldSettlements();

        return Ok(settlements);
    }

    /// <summary>
    /// Gets workflow status summary for a customer.
    /// </summary>
    [HttpGet("customers/{customerName}/status-summary")]
    public ActionResult<CustomerStatusSummaryResponse> GetCustomerStatusSummary(string customerName)
    {
        var summary = _agentService.GetCustomerStatusSummary(customerName);

        return Ok(summary);
    }

    /// <summary>
    /// Returns a RuleBased or Ollama-generated natural-language answer from current in-memory workflow data.
    /// </summary>
    [HttpPost("query")]
    public async Task<ActionResult<AgentQueryResponse>> Query(
        AgentQueryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _agentService.QueryAsync(request, cancellationToken);

        return Ok(response);
    }
}
