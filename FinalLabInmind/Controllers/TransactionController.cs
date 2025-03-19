using FinalLabInmind;
using FinalLabInmind.DbContext;
using Microsoft.AspNetCore.Mvc;
using FinalLabInmind.Interfaces;
using FinalLabInmind.Services.TransactionLogService;
using LoggingMicroservice.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

[Route("api/transactions")]
[ApiController]
public class TransactionLogController : ControllerBase
{
    private readonly ITransactionLogService _transactionLogService;

    public TransactionLogController(ITransactionLogService transactionLogService)
    {
        _transactionLogService = transactionLogService;
    }

    [HttpPost]
    public async Task<IActionResult> LogTransaction([FromBody] TransactionLog transactionLog)
    {
        var result = await _transactionLogService.LogTransactionAsync(transactionLog);
        return Ok(new
        {
            result.Id,
            result.AccountId,
            result.TransactionType,
            result.Amount,
            result.Status,
            result.Timestamp
        });
    }

    [HttpGet("{accountId}")]
    public async Task<IActionResult> GetTransactionLogsForAccount(long accountId)
    {
        var transactionLogs = await _transactionLogService.GetTransactionLogsForAccountAsync(accountId);
        if (!transactionLogs.Any())
        {
            return NotFound("No transaction logs found for this account.");
        }

        return Ok(transactionLogs.Select(t => new
        {
            t.Id,
            t.TransactionType,
            t.Amount,
            t.Timestamp,
            t.Status
        }));
    }

    [HttpGet]
    [EnableQuery]
    public IActionResult GetTransactionLogs()
    {
        return Ok(_transactionLogService.GetTransactionLogs());
    }
}
