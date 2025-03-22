using FinalLabInmind.DbContext;
using FinalLabInmind.Models;
using LoggingMicroservice.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.EventHandlers;

public class TransactionEventHandler : INotificationHandler<TransactionEvent>
{
    private readonly IAppDbContext _context;

    public TransactionEventHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(TransactionEvent notification, CancellationToken cancellationToken)
    {
        _context.TransactionEvents.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        if (notification.EventType == "TransactionReverted")
        {
            await HandleRollback(notification, cancellationToken);
        }
    }

    private async Task HandleRollback(TransactionEvent revertEvent, CancellationToken cancellationToken)
    {
        var originalTransactionLog = await _context.TransactionLogs
            .FirstOrDefaultAsync(t => t.Id == revertEvent.TransactionId, cancellationToken);

        if (originalTransactionLog == null)
            throw new InvalidOperationException("Original transaction not found.");

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == originalTransactionLog.AccountId, cancellationToken);

        if (account == null)
            throw new InvalidOperationException("Associated account not found.");

        switch (originalTransactionLog.TransactionType)
        {
            case "Deposit":
                account.Balance -= originalTransactionLog.Amount;
                break;

            case "Withdrawal":
                account.Balance += originalTransactionLog.Amount;
                break;

            default:
                throw new InvalidOperationException("Unsupported transaction type for rollback.");
        }
        
        _context.Accounts.Update(account);

        var rollbackLog = new TransactionLog
        {
            AccountId = account.Id,
            TransactionType = "Rollback",
            Amount = originalTransactionLog.Amount,
            Status = "Completed",
            Details = $"Rollback of transaction {originalTransactionLog.Id}",
            Timestamp = DateTime.UtcNow
        };

        _context.TransactionLogs.Add(rollbackLog);

        await _context.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"Transaction rollback processed for transaction ID {originalTransactionLog.Id}");
    }
}