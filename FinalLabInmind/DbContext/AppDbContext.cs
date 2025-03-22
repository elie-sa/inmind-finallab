using FinalLabInmind.Models;
using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.DbContext;

public class AppDbContext: Microsoft.EntityFrameworkCore.DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TransactionEvent> TransactionEvents { get; set; }

    public DbSet<AccountEvent> AccountEvents { get; set; }
    
    public DbSet<TransactionLog> TransactionLogs { get; set; }
    public DbSet<Account> Accounts { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}