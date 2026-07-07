using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

/// <summary>
/// Creates demo data for local and Dev Tunnel portfolio demonstrations.
/// </summary>
[ApiController]
[Route("api/demo")]
public class DemoController : ControllerBase
{
    private readonly DemoDataService _demoDataService;

    public DemoController(DemoDataService demoDataService)
    {
        _demoDataService = demoDataService;
    }

    /// <summary>
    /// Clears in-memory data and recreates a complete demo workflow dataset.
    /// </summary>
    [HttpPost("reset")]
    public ActionResult<object> Reset()
    {
        var result = _demoDataService.Reset();

        return Ok(result);
    }
}
