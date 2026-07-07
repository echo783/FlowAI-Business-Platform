using FlowAI.Api.Models;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

/// <summary>
/// Provides read-only access to approval requests created during contract workflow.
/// </summary>
[ApiController]
[Route("api/approval-requests")]
public class ApprovalRequestsController : ControllerBase
{
    private readonly ApprovalRequestService _approvalRequestService;

    public ApprovalRequestsController(ApprovalRequestService approvalRequestService)
    {
        _approvalRequestService = approvalRequestService;
    }

    /// <summary>
    /// Gets all approval requests.
    /// </summary>
    [HttpGet]
    public ActionResult<List<ApprovalRequest>> GetAll()
    {
        var approvalRequests = _approvalRequestService.GetAll();

        return Ok(approvalRequests);
    }

    /// <summary>
    /// Gets a single approval request by id.
    /// </summary>
    [HttpGet("{id:int}")]
    public ActionResult<ApprovalRequest> GetById(int id)
    {
        var approvalRequest = _approvalRequestService.GetById(id);

        if (approvalRequest is null)
        {
            return NotFound();
        }

        return Ok(approvalRequest);
    }
}
