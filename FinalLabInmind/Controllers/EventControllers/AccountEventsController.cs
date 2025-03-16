using FinalLabInmind.DbContext;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.Controllers;

[Route("api/accountEvents")]
[ApiController]
public class AccountEventsController: ControllerBase
{
    private readonly IMediator _mediator;
    
    public AccountEventsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] TransactionEvent request)
    {
        await _mediator.Publish(request);
        return Ok(new { Message = "Event dispatched successfully" });
    }
    
    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetEvents(long transactionId, [FromServices] IAppDbContext context)
    {
        var events = await context.TransactionLogs
            .Where(t => t.Id == transactionId)
            .ToListAsync();

        return Ok(events);
    }
}