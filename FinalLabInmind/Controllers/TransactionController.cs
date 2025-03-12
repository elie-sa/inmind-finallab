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

        [HttpGet("{accountId}")]
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
    }