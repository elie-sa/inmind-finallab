using FinalLabInmind.DbContext;
using FinalLabInmind.Interfaces;
using FinalLabInmind.Services.AccountService;
using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FinalLabInmind.Tests.Accounts;

public class AccountServiceTests
    {
        private readonly AppDbContext _context;
        private readonly AccountService _accountService;
        private readonly Mock<IMessagePublisher> _messagePublisherMock;

        public AccountServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            _messagePublisherMock = new Mock<IMessagePublisher>();

            _accountService = new AccountService(_context, _messagePublisherMock.Object);
        }

        [Fact]
        public async Task CreateAccountAsync_WithValidData_ShouldCreateAccount()
        {
            // Arrange
            var account = new Account
            {
                CustomerId = 1,
                AccountName = "Test Account"
            };

            // Act
            var created = await _accountService.CreateAccountAsync(account);

            // Assert
            Assert.NotNull(created);
            Assert.Equal("Test Account", created.AccountName);
            Assert.Equal(0, created.Balance);
        }

        [Fact]
        public async Task CreateAccountAsync_WithMissingCustomerId_ShouldThrow()
        {
            var account = new Account
            {
                CustomerId = 0,
                AccountName = "Invalid"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _accountService.CreateAccountAsync(account));
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldReturnAccount_WhenExists()
        {
            // Arrange
            var account = new Account { CustomerId = 123, AccountName = "Elie" };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountService.GetAccountByIdAsync(account.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Elie", result.AccountName);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldThrow_WhenNotFound()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountService.GetAccountByIdAsync(999));
        }

        [Fact]
        public async Task GetAccountBalanceSummaryAsync_WithValidCustomer_ShouldReturnCorrectSummary()
        {
            // Arrange
            var account = new Account { CustomerId = 77, AccountName = "Main" };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            _context.TransactionLogs.AddRange(
                new TransactionLog
                {
                    AccountId = account.Id,
                    TransactionType = "Deposit",
                    Amount = 100,
                    Status = "Completed",
                    Timestamp = DateTime.UtcNow,
                    Details = "Initial deposit"
                },
                new TransactionLog
                {
                    AccountId = account.Id,
                    TransactionType = "Withdrawal",
                    Amount = 30,
                    Status = "Completed",
                    Timestamp = DateTime.UtcNow,
                    Details = "Initial deposit"
                });

            await _context.SaveChangesAsync();

            // Act
            var summary = await _accountService.GetAccountBalanceSummaryAsync(account.CustomerId);

            // Assert
            Assert.Single(summary);
            var result = summary.First();
            Assert.Equal(100, result.TotalDeposits);
            Assert.Equal(30, result.TotalWithdrawals);
            Assert.Equal(0, result.CurrentBalance);
        }

        [Fact]
        public async Task GetAccountBalanceSummaryAsync_ShouldThrow_WhenNoAccounts()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountService.GetAccountBalanceSummaryAsync(customerId: 100000));
        }

        [Fact]
        public async Task GetCommonTransactionsAsync_ShouldReturnCommon_WhenValidAccounts()
        {
            // Arrange
            var account1 = new Account { CustomerId = 5, AccountName = "A1" };
            var account2 = new Account { CustomerId = 5, AccountName = "A2" };
            _context.Accounts.AddRange(account1, account2);
            await _context.SaveChangesAsync();

            _context.TransactionLogs.AddRange(
                new TransactionLog
                {
                    AccountId = account1.Id,
                    TransactionType = "Deposit",
                    Amount = 200,
                    Status = "Completed",
                    Timestamp = DateTime.UtcNow,
                    Details = "Initial deposit"
                },
                new TransactionLog
                {
                    AccountId = account2.Id,
                    TransactionType = "Deposit",
                    Amount = 200,
                    Status = "Completed",
                    Timestamp = DateTime.UtcNow,
                    Details = "Initial deposit"
                });

            await _context.SaveChangesAsync();

            // Act
            var result = await _accountService.GetCommonTransactionsAsync(new List<long> { account1.Id, account2.Id });

            // Assert
            Assert.Single(result);
            Assert.Equal(200, result.First().Amount);
        }

        [Fact]
        public async Task GetCommonTransactionsAsync_ShouldReturnEmpty_WhenNoCommonTransactions()
        {
            var acc1 = new Account { CustomerId = 9, AccountName = "A1" };
            var acc2 = new Account { CustomerId = 9, AccountName = "A2" };
            _context.Accounts.AddRange(acc1, acc2);
            await _context.SaveChangesAsync();

            _context.TransactionLogs.AddRange(
                new TransactionLog
                {
                    AccountId = acc1.Id,
                    TransactionType = "Deposit",
                    Amount = 100,
                    Status = "Completed",
                    Timestamp = DateTime.UtcNow,
                    Details = "acc1"
                },
                new TransactionLog
                {
                    AccountId = acc2.Id,
                    TransactionType = "Withdrawal", // different type
                    Amount = 200,                  // different amount
                    Status = "Completed",
                    Timestamp = DateTime.UtcNow,
                    Details = "acc2"
                });

            await _context.SaveChangesAsync();

            var result = await _accountService.GetCommonTransactionsAsync(new List<long> { acc1.Id, acc2.Id });

            Assert.Empty(result);
        }
        
        [Fact]
        public async Task GetCommonTransactionsAsync_ShouldThrow_WithSingleAccount()
        {
            var list = new List<long> { 1 };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _accountService.GetCommonTransactionsAsync(list));
        }
    }