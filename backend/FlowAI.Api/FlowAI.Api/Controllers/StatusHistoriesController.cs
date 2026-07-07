using FlowAI.Api.Models;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

/// <summary>
/// Provides common status history records across contracts, approvals, work orders, and settlements.
/// </summary>
[ApiController]
[Route("api/status-histories")]
public class StatusHistoriesController : ControllerBase
{
    private readonly StatusHistoryService _statusHistoryService;

    public StatusHistoriesController(StatusHistoryService statusHistoryService)
    {
        _statusHistoryService = statusHistoryService;
    }

    /// <summary>
    /// Gets all status history records in latest-first order.
    /// </summary>
    [HttpGet]
    public ActionResult<List<StatusHistory>> GetAll()
    {
        var histories = _statusHistoryService.GetAll();

        return Ok(histories);
    }
}
