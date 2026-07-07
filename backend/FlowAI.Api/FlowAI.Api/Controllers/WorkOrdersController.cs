using FlowAI.Api.DTOs;
using FlowAI.Api.Models;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

/// <summary>
/// Manages work orders created from approved contracts.
/// </summary>
[ApiController]
[Route("api/work-orders")]
public class WorkOrdersController : ControllerBase
{
    private readonly WorkOrderService _workOrderService;

    public WorkOrdersController(WorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
    }

    /// <summary>
    /// Gets all work orders.
    /// </summary>
    [HttpGet]
    public ActionResult<List<WorkOrder>> GetAll()
    {
        var workOrders = _workOrderService.GetAll();

        return Ok(workOrders);
    }

    /// <summary>
    /// Gets a single work order by id.
    /// </summary>
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

    /// <summary>
    /// Creates a work order manually for an approved contract.
    /// </summary>
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

    /// <summary>
    /// Starts a created work order.
    /// </summary>
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

    /// <summary>
    /// Completes an in-progress work order.
    /// </summary>
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
