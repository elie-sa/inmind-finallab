using System.Globalization;
using System.Text;
using FinalLabInmind;
using FinalLabInmind.DbContext;
using FinalLabInmind.DTO;
using Microsoft.AspNetCore.Mvc;
using FinalLabInmind.Interfaces;
using FinalLabInmind.Resources;
using FinalLabInmind.Services.TransactionService;
using LoggingMicroservice.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

[Route("api/transactions")]
[ApiController]
public class TransactionLogController : ControllerBase
    {
        private readonly IAppDbContext _context;
        private readonly IMessagePublisher _messagePublisher;
        private readonly TransactionService _transactionService;
        private readonly IStringLocalizer<AccountDetails> _accountLocalizer;
        private readonly IStringLocalizer<TransactionDetails> _transactionLocalizer;
        
        public TransactionLogController(IAppDbContext context, IMessagePublisher messagePublisher, TransactionService transactionService, IStringLocalizer<AccountDetails> accountLocalizer, IStringLocalizer<TransactionDetails> transactionLocalizer)
        {
            _context = context;
            _accountLocalizer = accountLocalizer;
            _transactionLocalizer = transactionLocalizer;
            _messagePublisher = messagePublisher;
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<IActionResult> LogTransaction([FromBody] TransactionLog transactionLog)
        {
            if (transactionLog == null)
            {
                throw new ArgumentNullException(nameof(transactionLog));
            }

            transactionLog.Timestamp = DateTime.UtcNow;

            _context.TransactionLogs.Add(transactionLog);
            await _context.SaveChangesAsync();
            
            await _messagePublisher.PublishTransactionAsync(transactionLog);

            return Ok(new
            {
                transactionLog.Id,
                transactionLog.AccountId,
                transactionLog.TransactionType,
                transactionLog.Amount,
                transactionLog.Status,
                transactionLog.Timestamp
            });
        }

        [HttpGet("transactionLog/{accountId}")]
        public async Task<IActionResult> GetTransactionLogsForAccount(long accountId)
        {
            var transactionLogs = await _context.TransactionLogs
                .Where(t => t.AccountId == accountId)
                .ToListAsync();

            if (transactionLogs == null || !transactionLogs.Any())
            {
                throw new ArgumentNullException(nameof(transactionLogs));
            }
            
            foreach (var transactionLog in transactionLogs)
            {

                await _messagePublisher.PublishTransactionAsync(transactionLog);
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
        public async Task<IQueryable<TransactionLog>> GetTransactionLogs()
        {
            var transactionLogs = _context.TransactionLogs;
            foreach (var transactionLog in transactionLogs)
            {

                await _messagePublisher.PublishTransactionAsync(transactionLog);
            }
            
            return transactionLogs;
        }
        
        [HttpGet("GetCommonTransactions")]
        public async Task<IActionResult> GetCommonTransactions([FromQuery] List<long> accountIds)
        {
            if (accountIds == null || accountIds.Count < 2)
            {
                return BadRequest("At least two account IDs must be provided.");
            }

            var transactions = await _context.TransactionLogs
                .Where(t => accountIds.Contains(t.AccountId))
                .ToListAsync();

            var commonTransactions = transactions
                .GroupBy(t => new { t.TransactionType, t.Amount })
                .Where(g => g.Count() == accountIds.Count)
                .Select(g => g.FirstOrDefault())
                .ToList();

            return Ok(commonTransactions);
        }
        
        [HttpGet("GetAccountBalanceSummary")]
        public async Task<IActionResult> GetAccountBalanceSummary([FromQuery] long customerId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();

            if (!accounts.Any())
            {
                throw new ArgumentException("No accounts found for the specified customer.");
            }

            var transactions = await _context.TransactionLogs
                .Where(t => accounts.Select(a => a.Id).Contains(t.AccountId))
                .ToListAsync();

            var balanceSummary = accounts.Select(account => new
            {
                AccountId = account.Id,
                AccountName = account.AccountName,
                TotalDeposits = transactions
                    .Where(t => t.AccountId == account.Id && t.TransactionType == "Deposit")
                    .Sum(t => t.Amount),
                TotalWithdrawals = transactions
                    .Where(t => t.AccountId == account.Id && t.TransactionType == "Withdrawal")
                    .Sum(t => t.Amount),
                CurrentBalance = transactions
                    .Where(t => t.AccountId == account.Id)
                    .Sum(t => t.TransactionType == "Deposit" ? t.Amount : -t.Amount)
            }).ToList();

            return Ok(balanceSummary);
        }
        
        [HttpPost]
        [Route("createAccount")]
        public async Task<IActionResult> CreateAccount([FromBody] Account account)
        {
            if (account == null || account.CustomerId == 0 || string.IsNullOrEmpty(account.AccountName))
            {
                throw new ArgumentNullException("Account data is required.");
            }

            _context.Accounts.Add(account);

            try
            {
                await _context.SaveChangesAsync();

                return Ok(account); 
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error while creating account: {ex.Message}");
            }
        }
        
        [HttpGet("account/{id}")]
        public async Task<ActionResult<Account>> GetAccountById(long id)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                throw new ArgumentNullException(nameof(Account));
            }

            return account;
        }
        
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferFunds([FromBody] TransferRequest request)
        {
            var result = await _transactionService.TransferFundsAsync(request.FromAccountId, request.ToAccountId, request.Amount);
        
            if (result.StartsWith("Transfer failed"))
            {
                return BadRequest(new { error = result });
            }

            return Ok(new { message = result });
        }
        
        
        [Route("accounts/{id}/details")]
        [HttpGet]
        public async Task<ActionResult<string>> GetAccountDetailsById(long id)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound(new
                {
                    name = "Account",
                    value = "Not found",
                    resourceNotFound = true,
                    searchedLocation = $"Account with ID {id} not found"
                });
            }

            var userLanguage = Request.Headers["Accept-Language"].ToString();

            if (string.IsNullOrEmpty(userLanguage))
            {
                userLanguage = "en";
            }

            try
            {
                var culture = new CultureInfo(userLanguage);
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (CultureNotFoundException)
            {
                return BadRequest("Invalid language code.");
            }

            string resourceKey = account.AccountName;
            var localizedName = _accountLocalizer[resourceKey];

            if (string.IsNullOrEmpty(localizedName))
            {
                localizedName = _accountLocalizer["Account_Default_Details"];
            }

            return Ok(localizedName);
        }
        
        [Route("accounts/{id}/notification")]
        [HttpPost]
        public async Task<ActionResult<string>> PostTransactionNotification(long id)
        {
            var transaction = await _context.TransactionLogs.FindAsync(id);

            if (transaction == null)
            {
                return NotFound(new
                {
                    name = "Transaction",
                    value = "Not found",
                    resourceNotFound = true,
                    searchedLocation = $"Transaction with ID {id} not found"
                });
            }

            var userLanguage = Request.Headers["Accept-Language"].ToString();

            if (string.IsNullOrEmpty(userLanguage))
            {
                userLanguage = "en";
            }

            try
            {
                var culture = new CultureInfo(userLanguage);
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (CultureNotFoundException)
            {
                return BadRequest("Invalid language code.");
            }

            string resourceKey = transaction.Details;
            var localizedName = _transactionLocalizer[resourceKey];

            if (string.IsNullOrEmpty(localizedName))
            {
                localizedName = _transactionLocalizer["Transaction_Default_Details"];
            }

            return Ok(localizedName);
        }
        
    }