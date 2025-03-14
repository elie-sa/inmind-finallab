using FinalLabInmind.DbContext;
using LoggingMicroservice.Models;

namespace FinalLabInmind.Repositories.TransactionRepository;

public class TransactionRepository : ITransactionRepository
{
    private readonly IAppDbContext _context;

    public TransactionRepository(IAppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TransactionLog transaction)
    {
        await _context.TransactionLogs.AddAsync(transaction);
    }
}