using FinalLabInmind.Models;
using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.DbContext;

public interface IAppDbContext
{
    DbSet<TransactionLog> TransactionLogs { get; }
    DbSet<Account> Accounts { get; }
    DbSet<TransactionEvent> TransactionEvents { get; set; }
    DbSet<AccountEvent> AccountEvents { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}