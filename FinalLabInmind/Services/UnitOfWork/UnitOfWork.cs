using FinalLabInmind.DbContext;
using FinalLabInmind.Repositories.AccountRepository;
using FinalLabInmind.Repositories.TransactionRepository;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinalLabInmind.Services.UnitOfWork;
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction _transaction;

    public IAccountRepository Accounts { get; }
    public ITransactionRepository Transactions { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Accounts = new AccountRepository(context);
        Transactions = new TransactionRepository(context);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
