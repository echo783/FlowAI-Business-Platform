using FlowAI.Api.DTOs;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public ActionResult<DashboardSummaryResponse> GetSummary()
    {
        var summary = _dashboardService.GetSummary();

        return Ok(summary);
    }
}
