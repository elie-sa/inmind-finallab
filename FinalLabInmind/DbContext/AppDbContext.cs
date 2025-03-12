using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.DbContext;

public class AppDbContext: Microsoft.EntityFrameworkCore.DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TransactionLog> TransactionLogs { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}