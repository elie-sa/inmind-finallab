using FinalLabInmind;
using FinalLabInmind.DbContext;
using FinalLabInmind.DTOs;
using Microsoft.AspNetCore.Mvc;
using FinalLabInmind.Interfaces;
using FinalLabInmind.Services.TransactionLogService;
using FinalLabInmind.Services.TransactionService;
using LoggingMicroservice.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

[Route("transaction-logs")]
[ApiController]
public class TransactionLogController : ControllerBase
{
    private readonly ITransactionLogService _transactionLogService;
    private readonly ITransactionService _transactionService;

    public TransactionLogController(ITransactionLogService transactionLogService, ITransactionService transactionService)
    {
        _transactionLogService = transactionLogService;
        _transactionService = transactionService;
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
        return Ok(result);
    }

    // Odata
    [HttpGet]
    [EnableQuery]
    public IActionResult GetTransactionLogs()
    {
        var result = _transactionLogService.GetTransactionLogs();
        return Ok(result);
    }
    
    [HttpPost("transfer")]
    public async Task<IActionResult> TransferFunds([FromBody] TransferRequestDto request)
    {
        var result = await _transactionService.TransferFundsAsync(request.FromAccountId, request.ToAccountId, request.Amount);

        if (result.Contains("successful"))
            return Ok(new { Message = result });

        return BadRequest(new { Message = result });
    }

}
