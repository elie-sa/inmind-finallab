using FinalLabInmind.DbContext;
using FinalLabInmind.EventHandlers;
using FinalLabInmind.Models;
using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.Tests.EventRollbackTests;

public class TransactionRollbackTests
{
    private readonly AppDbContext _context;
    private readonly TransactionEventHandler _handler;

    public TransactionRollbackTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _handler = new TransactionEventHandler(_context);
    }

    [Fact]
    public async Task ShouldRollbackDepositTransaction()
    {
        // Arrange
        var account = new Account { AccountName = "DepositAccount", CustomerId = 10, Balance = 500 };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var tx = new TransactionLog
        {
            AccountId = account.Id,
            TransactionType = "Deposit",
            Amount = 100,
            Status = "Completed",
            Details = "Test",
            Timestamp = DateTime.UtcNow
        };

        _context.TransactionLogs.Add(tx);
        await _context.SaveChangesAsync();

        var revertEvent = new TransactionEvent
        {
            TransactionId = tx.Id,
            EventType = "TransactionReverted",
            Details = "Revert",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _handler.Handle(revertEvent, default);

        // Assert
        var updated = await _context.Accounts.FindAsync(account.Id);
        Assert.Equal(400, updated.Balance);
    }

    [Fact]
    public async Task ShouldRollbackWithdrawalTransaction()
    {
        // Arrange
        var account = new Account { AccountName = "WithdrawAccount", CustomerId = 20, Balance = 200 };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var tx = new TransactionLog
        {
            AccountId = account.Id,
            TransactionType = "Withdrawal",
            Amount = 50,
            Status = "Completed",
            Details = "Test",
            Timestamp = DateTime.UtcNow
        };

        _context.TransactionLogs.Add(tx);
        await _context.SaveChangesAsync();

        var revertEvent = new TransactionEvent
        {
            TransactionId = tx.Id,
            EventType = "TransactionReverted",
            Details = "Revert",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _handler.Handle(revertEvent, default);

        // Assert
        var updated = await _context.Accounts.FindAsync(account.Id);
        Assert.Equal(250, updated.Balance);
    }
}
