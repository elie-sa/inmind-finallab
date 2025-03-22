using System.Text.Json;
using FinalLabInmind.DbContext;
using FinalLabInmind.EventHandlers;
using FinalLabInmind.Models;
using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.Tests.EventRollbackTests;

public class AccountRollbackTests
{
    private readonly AppDbContext _context;
    private readonly AccountEventHandler _handler;

    public AccountRollbackTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _handler = new AccountEventHandler(_context);
    }

    [Fact]
    public async Task ShouldRollbackAccountCreation()
    {
        // Arrange
        var account = new Account { AccountName = "Test", Balance = 0, CustomerId = 1 };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var creationEvent = new AccountEvent
        {
            AccountId = account.Id,
            EventType = "AccountCreated",
            Details = JsonSerializer.Serialize(account),
            Timestamp = DateTime.UtcNow.AddMinutes(-2)
        };

        var revertEvent = new AccountEvent
        {
            AccountId = account.Id,
            EventType = "AccountReverted",
            Details = "Rollback",
            Timestamp = DateTime.UtcNow
        };

        _context.AccountEvents.Add(creationEvent);
        await _context.SaveChangesAsync();

        // Act
        await _handler.Handle(revertEvent, default);

        // Assert
        var acc = await _context.Accounts.FindAsync(account.Id);
        Assert.Null(acc);
    }

    [Fact]
    public async Task ShouldRollbackAccountBalanceUpdate()
    {
        // Arrange
        var account = new Account { AccountName = "BalanceTest", Balance = 200, CustomerId = 2 };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var updateEvent = new AccountEvent
        {
            AccountId = account.Id,
            EventType = "AccountBalanceUpdated",
            Details = "150",
            Timestamp = DateTime.UtcNow.AddMinutes(-1)
        };

        var revertEvent = new AccountEvent
        {
            AccountId = account.Id,
            EventType = "AccountReverted",
            Details = "Rollback",
            Timestamp = DateTime.UtcNow
        };

        _context.AccountEvents.Add(updateEvent);
        await _context.SaveChangesAsync();

        // Act
        await _handler.Handle(revertEvent, default);

        // Assert
        var updated = await _context.Accounts.FindAsync(account.Id);
        Assert.Equal(150, updated.Balance);
    }
}