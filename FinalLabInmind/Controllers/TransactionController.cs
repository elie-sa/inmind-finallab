using FinalLabInmind;
using FinalLabInmind.DbContext;
using FinalLabInmind.DTOs;
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
    public async Task<IActionResult> LogTransaction([FromBody] TransactionLogDto transactionLogDto)
    {
        var result = await _transactionLogService.LogTransactionAsync(transactionLogDto);
        return Ok(result);
    }

    [HttpGet("{accountId}")]
    public async Task<IActionResult> GetTransactionLogsForAccount(long accountId)
    {
        var result = await _transactionLogService.GetTransactionLogsForAccountAsync(accountId);
        return result.Any() ? Ok(result) : NotFound("No transaction logs found for this account.");
    }

    [HttpGet]
    [EnableQuery]
    public IActionResult GetTransactionLogs()
    {
        return Ok(_transactionLogService.GetTransactionLogs());
    }
}

