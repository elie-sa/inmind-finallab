using FinalLabInmind.DbContext;
using LoggingMicroservice.Models;
using MediatR;

namespace FinalLabInmind;

public class AccountEventHandler : INotificationHandler<AccountEvent>
{
    private readonly IAppDbContext _context;

    public AccountEventHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AccountEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Arrived at {notification.Timestamp}");
        // just to show that we are sending the event with its attributes
        
        var account = new Account
        {
            Id = notification.AccountId,
            CustomerId = notification.CustomerId,
            AccountName = notification.CustomerName,
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
    }
}