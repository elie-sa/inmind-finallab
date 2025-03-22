using FinalLabInmind.Services.AccountLocalizationService;
using FinalLabInmind.Services.AccountService;
using LoggingMicroservice.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinalLabInmind.Controllers;

[Route("accounts")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IAccountLocalizationService _accountLocalizationService;
    
    public AccountController(IAccountService accountService, IAccountLocalizationService accountLocalizationService)
    {
        _accountService = accountService;
        _accountLocalizationService = accountLocalizationService;
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
    
    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetAccountDetails(long id)
    {
        var userLanguage = Request.Headers["Accept-Language"].ToString();
        if (string.IsNullOrEmpty(userLanguage))
        {
            userLanguage = "en";
        }

        var localizedDetails = await _accountLocalizationService.GetLocalizedAccountDetailsAsync(id, userLanguage);
        return Ok(localizedDetails);
    }
}