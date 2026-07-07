using FlowAI.Api.DTOs;
using FlowAI.Api.Models;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly AgentService _agentService;

    public AgentController(AgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpGet("contracts/pending-approval")]
    public ActionResult<List<Contract>> GetPendingApprovalContracts()
    {
        var contracts = _agentService.GetPendingApprovalContracts();

        return Ok(contracts);
    }

    [HttpGet("workorders/today")]
    public ActionResult<List<WorkOrder>> GetTodayWorkOrders()
    {
        var workOrders = _agentService.GetTodayWorkOrders();

        return Ok(workOrders);
    }

    [HttpGet("workorders/delayed")]
    public ActionResult<List<WorkOrder>> GetDelayedWorkOrders()
    {
        var workOrders = _agentService.GetDelayedWorkOrders();

        return Ok(workOrders);
    }

    [HttpGet("settlements/on-hold")]
    public ActionResult<List<Settlement>> GetOnHoldSettlements()
    {
        var settlements = _agentService.GetOnHoldSettlements();

        return Ok(settlements);
    }

    [HttpGet("customers/{customerName}/status-summary")]
    public ActionResult<CustomerStatusSummaryResponse> GetCustomerStatusSummary(string customerName)
    {
        var summary = _agentService.GetCustomerStatusSummary(customerName);

        return Ok(summary);
    }

    [HttpPost("query")]
    public ActionResult<AgentQueryResponse> Query(AgentQueryRequest request)
    {
        var response = _agentService.Query(request);

        return Ok(response);
    }
}
