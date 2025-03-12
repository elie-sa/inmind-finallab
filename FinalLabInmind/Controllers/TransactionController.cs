using FinalLabInmind;
using FinalLabInmind.DbContext;
using Microsoft.AspNetCore.Mvc;
using FinalLabInmind.Interfaces;
using LoggingMicroservice.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

[Route("api/transactions")]
[ApiController]
public class TransactionLogController : ControllerBase
    {
        private readonly IAppDbContext _context;
        private readonly IMessagePublisher _messagePublisher;

        public TransactionLogController(IAppDbContext context, IMessagePublisher messagePublisher)
        {
            _context = context;
            _messagePublisher = messagePublisher;
        }

        [HttpPost]
        public async Task<IActionResult> LogTransaction([FromBody] TransactionLog transactionLog)
        {
            if (transactionLog == null)
            {
                return BadRequest("Transaction data is missing.");
            }

            transactionLog.Timestamp = DateTime.UtcNow;

            _context.TransactionLogs.Add(transactionLog);
            await _context.SaveChangesAsync();
            
            _messagePublisher.PublishTransactionAsync(transactionLog);

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
                return NotFound("No transaction logs found for this account.");
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
        public IQueryable<TransactionLog> GetTransactionLogs()
        {
            return _context.TransactionLogs;
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
                return NotFound("No accounts found for the specified customer.");
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
                return BadRequest("Account data is required.");
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
                return NotFound();
            }

            return account;
        }
    }