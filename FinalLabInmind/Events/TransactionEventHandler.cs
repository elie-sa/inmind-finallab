using FinalLabInmind.DbContext;
using LoggingMicroservice.Models;
using MediatR;

namespace FinalLabInmind;

public class TransactionEventHandler : INotificationHandler<TransactionEvent>
{
    private readonly IAppDbContext _context;

    public TransactionEventHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(TransactionEvent notification, CancellationToken cancellationToken)
    {
        var log = new TransactionLog
        {
            AccountId = notification.AccountId,
            TransactionType = notification.EventType,
            Amount = notification.Amount,
            Timestamp = notification.Timestamp,
            Status = notification.Status,
            Details = notification.Details
        };

        _context.TransactionLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}