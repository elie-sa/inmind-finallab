using LoggingMicroservice.DbContext;
using LoggingMicroservice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoggingMicroservice.Controllers;

[ApiController]
[Route("[controller]")]
public class LoggingController:  ControllerBase
{
    private readonly IAppDbContext _context;

    public LoggingController(IAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context), "AppDbContext is null!");
    }
    
    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] Guid? requestId,
        [FromQuery] string? routeUrl,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        IQueryable<Log> query = _context.Logs;

        if (requestId.HasValue)
        {
            query = query.Where(log => log.RequestId == requestId.Value);
        }

        if (!string.IsNullOrEmpty(routeUrl))
        {
            query = query.Where(log => log.RouteURL.Contains(routeUrl));
        }

        if (startDate.HasValue)
        {
            query = query.Where(log => log.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(log => log.Timestamp <= endDate.Value);
        }

        //pagination
        var logs = await query
            .OrderByDescending(log => log.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(logs.Select(log => new
        {
            log.RequestId,
            log.RouteURL,
            log.Timestamp,
            log.RequestObject
        }));
    }
    
}