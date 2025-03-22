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
                throw new Exception("Account not found.");
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
                    throw new Exception("Insufficient funds for withdrawal.");
                }
            }
            else
            {
                throw new Exception("Invalid transaction type.");
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
                throw new Exception("No transaction logs found for this account.");
            }

            return transactionLogs.Select(t => new TransactionLogDto(t)).ToList();
        }

        public IQueryable<TransactionLogDto> GetTransactionLogs()
        {
            return _context.TransactionLogs.Select(t => new TransactionLogDto(t));
        }

        public async Task<List<TransactionLog>> GetCommonTransactionsAsync(List<long> accountIds)
        {
            if (accountIds == null || accountIds.Count < 2)
            {
                throw new Exception("At least two account IDs must be provided.");
            }

            var transactions = await _context.TransactionLogs
                .Where(t => accountIds.Contains(t.AccountId))
                .ToListAsync();

            var commonTransactions = transactions
                .GroupBy(t => new { t.TransactionType, t.Amount })
                .Where(g => g.Count() == accountIds.Count)
                .Select(g => g.First())
                .ToList();

            return commonTransactions;
        }

        public async Task<List<AccountBalanceSummaryDto>> GetAccountBalanceSummaryAsync(long customerId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();

            if (!accounts.Any())
            {
                throw new Exception("No accounts found for the specified customer.");
            }

            var transactions = await _context.TransactionLogs
                .Where(t => accounts.Select(a => a.Id).Contains(t.AccountId))
                .ToListAsync();

            var balanceSummary = accounts.Select(account => new AccountBalanceSummaryDto
            {
                AccountId = account.Id,
                AccountName = account.AccountName,
                TotalDeposits = transactions
                    .Where(t => t.AccountId == account.Id && t.TransactionType.Equals("Deposit", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Amount),
                TotalWithdrawals = transactions
                    .Where(t => t.AccountId == account.Id && t.TransactionType.Equals("Withdrawal", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Amount),
                CurrentBalance = account.Balance
            }).ToList();

            return balanceSummary;
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            if (account == null || account.CustomerId == 0 || string.IsNullOrEmpty(account.AccountName))
            {
                throw new Exception("Account data is required.");
            }

            account.Balance = 0;
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return account;
        }

        public async Task<Account> GetAccountByIdAsync(long id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                throw new Exception("Account not found.");
            }
            return account;
        }
    }