using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace LoggingMicroservice.DbContext;

public class AppDbContext: Microsoft.EntityFrameworkCore.DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Log> Logs { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}