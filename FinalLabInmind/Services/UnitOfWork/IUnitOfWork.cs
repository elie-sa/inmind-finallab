using FinalLabInmind.DbContext;
using FinalLabInmind.Repositories.AccountRepository;
using FinalLabInmind.Repositories.TransactionRepository;
using Microsoft.EntityFrameworkCore.Storage;

public interface IUnitOfWork : IDisposable
{
    IAccountRepository Accounts { get; }
    ITransactionRepository Transactions { get; }
    Task<int> SaveAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
