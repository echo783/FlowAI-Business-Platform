using FlowAI.Api.DTOs;
using FlowAI.Api.Models;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

/// <summary>
/// Manages settlement request, review, approval, hold, and rejection workflow.
/// </summary>
[ApiController]
[Route("api/settlements")]
public class SettlementsController : ControllerBase
{
    private readonly SettlementService _settlementService;

    public SettlementsController(SettlementService settlementService)
    {
        _settlementService = settlementService;
    }

    /// <summary>
    /// Gets all settlements.
    /// </summary>
    [HttpGet]
    public ActionResult<List<Settlement>> GetAll()
    {
        var settlements = _settlementService.GetAll();

        return Ok(settlements);
    }

    /// <summary>
    /// Gets a single settlement by id.
    /// </summary>
    [HttpGet("{id:int}")]
    public ActionResult<Settlement> GetById(int id)
    {
        var settlement = _settlementService.GetById(id);

        if (settlement is null)
        {
            return NotFound();
        }

        return Ok(settlement);
    }

    /// <summary>
    /// Requests settlement for a completed work order.
    /// </summary>
    [HttpPost]
    public ActionResult<Settlement> Create(CreateSettlementRequest request)
    {
        var result = _settlementService.Create(request);

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
            new { id = result.Settlement!.Id },
            result.Settlement
        );
    }

    /// <summary>
    /// Starts review for a requested settlement.
    /// </summary>
    [HttpPost("{id:int}/review")]
    public ActionResult<Settlement> Review(int id)
    {
        var result = _settlementService.Review(id);

        if (result.IsNotFound)
        {
            return NotFound(result.Error);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Settlement);
    }

    /// <summary>
    /// Approves a settlement that is under review.
    /// </summary>
    [HttpPost("{id:int}/approve")]
    public ActionResult<Settlement> Approve(int id)
    {
        var result = _settlementService.Approve(id);

        if (result.IsNotFound)
        {
            return NotFound(result.Error);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Settlement);
    }

    /// <summary>
    /// Holds a settlement that is under review.
    /// </summary>
    [HttpPost("{id:int}/hold")]
    public ActionResult<Settlement> Hold(int id, HoldSettlementRequest request)
    {
        var result = _settlementService.Hold(id, request);

        if (result.IsNotFound)
        {
            return NotFound(result.Error);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Settlement);
    }

    /// <summary>
    /// Rejects a settlement that is under review.
    /// </summary>
    [HttpPost("{id:int}/reject")]
    public ActionResult<Settlement> Reject(int id, RejectSettlementRequest request)
    {
        var result = _settlementService.Reject(id, request);

        if (result.IsNotFound)
        {
            return NotFound(result.Error);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Settlement);
    }
}
