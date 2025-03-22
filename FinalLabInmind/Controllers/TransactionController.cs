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
        return Ok(result);
    }

    [HttpGet]
    [EnableQuery]
    public IActionResult GetTransactionLogs()
    {
        var result = _transactionLogService.GetTransactionLogs();
        return Ok(result);
    }

    [HttpGet("GetCommonTransactions")]
    public async Task<IActionResult> GetCommonTransactions([FromQuery] List<long> accountIds)
    {
        var transactions = await _transactionLogService.GetCommonTransactionsAsync(accountIds);
        return Ok(transactions);
    }

    [HttpGet("GetAccountBalanceSummary")]
    public async Task<IActionResult> GetAccountBalanceSummary([FromQuery] long customerId)
    {
        var summary = await _transactionLogService.GetAccountBalanceSummaryAsync(customerId);
        return Ok(summary);
    }

    [HttpPost("createAccount")]
    public async Task<IActionResult> CreateAccount([FromBody] Account account)
    {
        var createdAccount = await _transactionLogService.CreateAccountAsync(account);
        return Ok(createdAccount);
    }

    [HttpGet("account/{id}")]
    public async Task<IActionResult> GetAccountById(long id)
    {
        var account = await _transactionLogService.GetAccountByIdAsync(id);
        return Ok(account);
    }
}

