using FlowAI.Api.DTOs;
using FlowAI.Api.Models;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

[ApiController]
[Route("api/work-orders")]
public class WorkOrdersController : ControllerBase
{
    private readonly WorkOrderService _workOrderService;

    public WorkOrdersController(WorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
    }

    [HttpGet]
    public ActionResult<List<WorkOrder>> GetAll()
    {
        var workOrders = _workOrderService.GetAll();

        return Ok(workOrders);
    }

    [HttpGet("{id:int}")]
    public ActionResult<WorkOrder> GetById(int id)
    {
        var workOrder = _workOrderService.GetById(id);

        if (workOrder is null)
        {
            return NotFound();
        }

        return Ok(workOrder);
    }

    [HttpPost]
    public ActionResult<WorkOrder> Create(CreateWorkOrderRequest request)
    {
        var result = _workOrderService.Create(request);

        if (result.IsNotFound)
        {
            return NotFound(result.Error);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.WorkOrder!.Id },
            result.WorkOrder
        );
    }

    [HttpPost("{id:int}/start")]
    public ActionResult<WorkOrder> Start(int id)
    {
        var result = _workOrderService.Start(id);

        if (result.IsNotFound)
        {
            return NotFound(result.Error);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.WorkOrder);
    }

    [HttpPost("{id:int}/complete")]
    public ActionResult<WorkOrder> Complete(int id)
    {
        var result = _workOrderService.Complete(id);

        if (result.IsNotFound)
        {
            return NotFound(result.Error);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.WorkOrder);
    }
}
