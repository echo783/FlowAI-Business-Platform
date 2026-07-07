using FlowAI.Api.Models;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

[ApiController]
[Route("api/approval-requests")]
public class ApprovalRequestsController : ControllerBase
{
    private readonly ApprovalRequestService _approvalRequestService;

    public ApprovalRequestsController(ApprovalRequestService approvalRequestService)
    {
        _approvalRequestService = approvalRequestService;
    }

    [HttpGet]
    public ActionResult<List<ApprovalRequest>> GetAll()
    {
        var approvalRequests = _approvalRequestService.GetAll();

        return Ok(approvalRequests);
    }

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
