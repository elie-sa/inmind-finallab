using FinalLabInmind.DbContext;
using FinalLabInmind.DTOs;
using FinalLabInmind.Interfaces;
using FinalLabInmind.Services.TransactionLogService;
using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FinalLabInmind.Tests.Transactions;

public class TransactionServiceTests
{
    private readonly AppDbContext _context;
    private readonly TransactionLogService _transactionService;
    private readonly Mock<IMessagePublisher> _messagePublisherMock;

    public TransactionServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _messagePublisherMock = new Mock<IMessagePublisher>();
        _transactionService = new TransactionLogService(_context, _messagePublisherMock.Object);
    }

    [Fact]
    public async Task LogTransactionAsync_Deposit_ShouldSucceed()
    {
        // Arrange
        var account = new Account { CustomerId = 1, AccountName = "Main" };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var dto = new TransactionLogDto
        {
            AccountId = account.Id,
            TransactionType = "Deposit",
            Amount = 100,
            Status = "Completed",
            Details = "Salary"
        };

        // Act
        var result = await _transactionService.LogTransactionAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Deposit", result.TransactionType);
        Assert.Equal(100, result.Amount);
    }

    [Fact]
    public async Task LogTransactionAsync_Withdrawal_WithSufficientFunds_ShouldSucceed()
    {
        // Arrange
        var account = new Account { CustomerId = 2, AccountName = "Saver", Balance = 200 };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var dto = new TransactionLogDto
        {
            AccountId = account.Id,
            TransactionType = "Withdrawal",
            Amount = 150,
            Status = "Completed",
            Details = "ATM"
        };

        // Act
        var result = await _transactionService.LogTransactionAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Withdrawal", result.TransactionType);
        Assert.Equal(150, result.Amount);
    }

    [Fact]
    public async Task LogTransactionAsync_Withdrawal_WithInsufficientFunds_ShouldThrow()
    {
        // Arrange
        var account = new Account { CustomerId = 3, AccountName = "Empty", Balance = 50 };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var dto = new TransactionLogDto
        {
            AccountId = account.Id,
            TransactionType = "Withdrawal",
            Amount = 100,
            Status = "Failed",
            Details = "Overdraft"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _transactionService.LogTransactionAsync(dto));
    }

    [Fact]
    public async Task LogTransactionAsync_InvalidAccount_ShouldThrow()
    {
        // Arrange
        var dto = new TransactionLogDto
        {
            AccountId = 999,
            TransactionType = "Deposit",
            Amount = 50,
            Status = "Failed",
            Details = "Ghost"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _transactionService.LogTransactionAsync(dto));
    }

    [Fact]
    public async Task GetTransactionLogsForAccountAsync_ShouldReturnLogs()
    {
        // Arrange
        var account = new Account { CustomerId = 4, AccountName = "Logger" };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        _context.TransactionLogs.Add(new TransactionLog
        {
            AccountId = account.Id,
            TransactionType = "Deposit",
            Amount = 200,
            Status = "Completed",
            Timestamp = DateTime.UtcNow,
            Details = "Bonus"
        });

        await _context.SaveChangesAsync();

        // Act
        var logs = await _transactionService.GetTransactionLogsForAccountAsync(account.Id);

        // Assert
        Assert.Single(logs);
        Assert.Equal(200, logs[0].Amount);
    }

    [Fact]
    public async Task GetTransactionLogsForAccountAsync_NoLogs_ShouldThrow()
    {
        // Arrange
        long nonExistingAccountId = 777;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _transactionService.GetTransactionLogsForAccountAsync(nonExistingAccountId));

        Assert.Equal("No transaction logs found for this account.", exception.Message);
    }
}