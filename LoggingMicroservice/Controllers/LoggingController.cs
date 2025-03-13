using LoggingMicroservice.DbContext;
using LoggingMicroservice.Models;
using LoggingMicroservice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoggingMicroservice.Controllers;

[ApiController]
[Route("[controller]")]
public class LoggingController:  ControllerBase
{
    private readonly IAppDbContext _context;
    private readonly LogQueue _logQueue;
    
    public LoggingController(IAppDbContext context, LogQueue logQueue)
    {
        _context = context;
        _logQueue = logQueue;
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
    
    [HttpPost]
    public async Task<IActionResult> StoreLogs()
    {
        var logs = _logQueue.DequeueAll();
        if (!logs.Any()) return BadRequest("No logs to store.");

        await _context.Logs.AddRangeAsync(logs);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"{logs.Count} logs stored successfully." });
    }
    
}