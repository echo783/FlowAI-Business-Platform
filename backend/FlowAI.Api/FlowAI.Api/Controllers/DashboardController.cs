using FlowAI.Api.DTOs;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

/// <summary>
/// Provides dashboard metrics for the workflow portfolio.
/// </summary>
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Gets total and status-based counts for contracts, work orders, and settlements.
    /// </summary>
    [HttpGet("summary")]
    public ActionResult<DashboardSummaryResponse> GetSummary()
    {
        var summary = _dashboardService.GetSummary();

        return Ok(summary);
    }
}
