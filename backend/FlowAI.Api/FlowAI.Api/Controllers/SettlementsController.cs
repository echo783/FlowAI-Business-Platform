using FlowAI.Api.DTOs;
using FlowAI.Api.Models;
using FlowAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowAI.Api.Controllers;

[ApiController]
[Route("api/settlements")]
public class SettlementsController : ControllerBase
{
    private readonly SettlementService _settlementService;

    public SettlementsController(SettlementService settlementService)
    {
        _settlementService = settlementService;
    }

    [HttpGet]
    public ActionResult<List<Settlement>> GetAll()
    {
        var settlements = _settlementService.GetAll();

        return Ok(settlements);
    }

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
}
