using FinalLabInmind.DbContext;
using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.Tests;

public class TestDbContext : Microsoft.EntityFrameworkCore.DbContext, IAppDbContext
{
    public DbSet<TransactionLog> TransactionLogs { get; set; }
    public DbSet<Account> Accounts { get; set; }

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
}
