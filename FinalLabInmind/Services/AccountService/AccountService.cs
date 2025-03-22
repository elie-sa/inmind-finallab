using FinalLabInmind.DbContext;
using FinalLabInmind.DTOs;
using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.Services.AccountService;

public class AccountService : IAccountService
    {
        private readonly IAppDbContext _context;

        public AccountService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            if (account == null || account.CustomerId == 0 || string.IsNullOrEmpty(account.AccountName))
            {
                throw new ArgumentNullException("Account data is required.");
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
                throw new ArgumentException("Account not found.");
            }
            return account;
        }

        public async Task<List<AccountBalanceSummaryDto>> GetAccountBalanceSummaryAsync(long customerId)
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
        
        public async Task<List<TransactionLog>> GetCommonTransactionsAsync(List<long> accountIds)
        {
            if (accountIds == null || accountIds.Count < 2)
            {
                throw new ArgumentException("At least two account IDs must be provided.");
            }

            var uniqueAccountIds = accountIds.Distinct().ToList();

            var transactions = await _context.TransactionLogs
                .Where(t => uniqueAccountIds.Contains(t.AccountId))
                .ToListAsync();

            var commonByType = transactions
                .GroupBy(t => t.TransactionType)
                .Where(g => g.Select(t => t.AccountId).Distinct().Count() == uniqueAccountIds.Count)
                .Select(g => g.First())
                .ToList();

            var commonByAmount = transactions
                .GroupBy(t => t.Amount)
                .Where(g => g.Select(t => t.AccountId).Distinct().Count() == uniqueAccountIds.Count)
                .Select(g => g.First())
                .ToList();

            var commonTransactions = commonByType.Union(commonByAmount).ToList();

            return commonTransactions;
        }
    }