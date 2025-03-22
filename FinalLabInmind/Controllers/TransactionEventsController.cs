using FinalLabInmind.DbContext;
using FinalLabInmind.DTOs;
using FinalLabInmind.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.Controllers;

[ApiController]
[Route("events")]
public class TransactionEventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionEventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> PostEvent([FromBody] EventDto eventDto, [FromQuery] long transactionId)
    {
        var transactionEvent = new TransactionEvent
        {
            TransactionId = transactionId,
            EventType = eventDto.EventType,
            Details = eventDto.Details,
            Timestamp = eventDto.Timestamp
        };

        await _mediator.Publish(transactionEvent);

        return Ok(new 
        { 
            Message = "Transaction event dispatched successfully.", 
            EventId = transactionEvent.Id 
        });
    }

    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetEvents([FromServices] IAppDbContext context, [FromRoute] long transactionId)
    {
        var events = await context.TransactionEvents
            .Where(e => e.TransactionId == transactionId)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();

        if (!events.Any())
            return NotFound("No events found for this transaction.");

        return Ok(events);
    }
}