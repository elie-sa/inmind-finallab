using FinalLabInmind.DbContext;
using FinalLabInmind.DTOs;
using FinalLabInmind.Interfaces;
using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.Services.TransactionLogService;

public class TransactionLogService : ITransactionLogService
    {
        private readonly IAppDbContext _context;
        private readonly IMessagePublisher _messagePublisher;

        public TransactionLogService(IAppDbContext context, IMessagePublisher messagePublisher)
        {
            _context = context;
            _messagePublisher = messagePublisher;
        }

        public async Task<TransactionLogDto> LogTransactionAsync(TransactionLogDto transactionLogDto)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == transactionLogDto.AccountId);
            if (account == null)
            {
                throw new ArgumentNullException("Account not found.");
            }

            decimal balanceChange = 0;
            if (transactionLogDto.TransactionType.Equals("Deposit", StringComparison.OrdinalIgnoreCase))
            {
                balanceChange = transactionLogDto.Amount;
            }
            else if (transactionLogDto.TransactionType.Equals("Withdrawal", StringComparison.OrdinalIgnoreCase))
            {
                balanceChange = -transactionLogDto.Amount;
                if (account.Balance + balanceChange < 0)
                {
                    throw new ArgumentException("Insufficient funds for withdrawal.");
                }
            }
            else
            {
                throw new ArgumentException("Invalid transaction type.");
            }

            account.Balance += balanceChange;

            var transactionLog = new TransactionLog
            {
                AccountId = transactionLogDto.AccountId,
                TransactionType = transactionLogDto.TransactionType,
                Amount = transactionLogDto.Amount,
                Status = transactionLogDto.Status,
                Timestamp = DateTime.UtcNow,
                Details = transactionLogDto.Details
            };

            _context.TransactionLogs.Add(transactionLog);

            await _context.SaveChangesAsync();

            await _messagePublisher.PublishTransactionAsync(transactionLog);

            return new TransactionLogDto(transactionLog);
        }

        public async Task<List<TransactionLogDto>> GetTransactionLogsForAccountAsync(long accountId)
        {
            var transactionLogs = await _context.TransactionLogs
                .Where(t => t.AccountId == accountId)
                .ToListAsync();

            if (!transactionLogs.Any())
            {
                throw new ArgumentException("No transaction logs found for this account.");
            }

            return transactionLogs.Select(t => new TransactionLogDto(t)).ToList();
        }

        public IQueryable<TransactionLogDto> GetTransactionLogs()
        {
            return _context.TransactionLogs.Select(t => new TransactionLogDto(t));
        }
        
    }