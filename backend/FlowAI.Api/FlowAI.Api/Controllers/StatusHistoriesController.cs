using FlowAI.Api.Models;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

[ApiController]
[Route("api/status-histories")]
public class StatusHistoriesController : ControllerBase
{
    private readonly StatusHistoryService _statusHistoryService;

    public StatusHistoriesController(StatusHistoryService statusHistoryService)
    {
        _statusHistoryService = statusHistoryService;
    }

    [HttpGet]
    public ActionResult<List<StatusHistory>> GetAll()
    {
        var histories = _statusHistoryService.GetAll();

        return Ok(histories);
    }
}
