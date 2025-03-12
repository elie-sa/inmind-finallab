using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace LoggingMicroservice.DbContext;

public interface IAppDbContext
{
    DbSet<Log> Logs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}