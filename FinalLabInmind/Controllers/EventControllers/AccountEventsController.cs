using FinalLabInmind.DbContext;
using FinalLabInmind.DTOs;
using FinalLabInmind.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.Controllers;

[ApiController]
[Route("account-events")]
public class AccountEventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountEventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> PostEvent([FromBody] EventDto eventDto, [FromQuery] long accountId)
    {
        var accountEvent = new AccountEvent
        {
            AccountId = accountId,
            EventType = eventDto.EventType,
            Details = eventDto.Details,
            Timestamp = eventDto.Timestamp
        };

        await _mediator.Publish(accountEvent);

        return Ok(new 
        { 
            Message = "Account event dispatched successfully.", 
            EventId = accountEvent.Id 
        });
    }

    [HttpGet("{accountId}")]
    public async Task<IActionResult> GetEvents([FromServices] IAppDbContext context, [FromRoute] long accountId)
    {
        var events = await context.AccountEvents
            .Where(e => e.AccountId == accountId)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();

        if (!events.Any())
            return NotFound("No events found for this account.");

        return Ok(events);
    }
}