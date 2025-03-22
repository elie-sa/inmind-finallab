using FinalLabInmind.Services.AccountService;
using LoggingMicroservice.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinalLabInmind.Controllers;

[Route("accounts")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] Account account)
    {
        var createdAccount = await _accountService.CreateAccountAsync(account);
        return Ok(createdAccount);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(long id)
    {
        var account = await _accountService.GetAccountByIdAsync(id);
        return Ok(account);
    }

    [HttpGet("balance-summary/{customerId}")]
    public async Task<IActionResult> GetAccountBalanceSummary(long customerId)
    {
        var summary = await _accountService.GetAccountBalanceSummaryAsync(customerId);
        return Ok(summary);
    }

    [HttpGet("common-transactions")]
    public async Task<IActionResult> GetCommonTransactions([FromQuery] List<long> accountIds)
    {
        var transactions = await _accountService.GetCommonTransactionsAsync(accountIds);
        return Ok(transactions);
    }
}