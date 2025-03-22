using System.Text.Json;
using FinalLabInmind.DbContext;
using FinalLabInmind.Models;
using LoggingMicroservice.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.EventHandlers;

public class AccountEventHandler : INotificationHandler<AccountEvent>
{
    private readonly IAppDbContext _context;

    public AccountEventHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AccountEvent notification, CancellationToken cancellationToken)
    {
        _context.AccountEvents.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        if (notification.EventType == "AccountReverted")
        {
            await HandleRollback(notification, cancellationToken);
        }
    }

    private async Task HandleRollback(AccountEvent revertEvent, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == revertEvent.AccountId, cancellationToken);

        var lastEvent = await _context.AccountEvents
            .Where(e => e.AccountId == revertEvent.AccountId &&
                        e.EventType != "AccountReverted" &&
                        e.Timestamp < revertEvent.Timestamp)
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastEvent == null)
            throw new InvalidOperationException("No previous event to revert to.");

        switch (lastEvent.EventType)
        {
            case "AccountCreated":
                if (account != null)
                    _context.Accounts.Remove(account);
                break;

            case "AccountDeleted":
                var previousData = JsonSerializer.Deserialize<Account>(lastEvent.Details);
                if (previousData != null)
                {
                    _context.Accounts.Add(previousData);
                }
                break;

            case "AccountBalanceUpdated":
                decimal previousBalance = decimal.Parse(lastEvent.Details);
                if (account != null)
                {
                    account.Balance = previousBalance;
                    _context.Accounts.Update(account);
                }
                break;

            default:
                throw new InvalidOperationException("Unsupported rollback event type.");
        }

        await _context.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"Rollback applied for Account ID {revertEvent.AccountId}");
    }
}