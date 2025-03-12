using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.DbContext;

public interface IAppDbContext
{
    DbSet<TransactionLog> TransactionLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}