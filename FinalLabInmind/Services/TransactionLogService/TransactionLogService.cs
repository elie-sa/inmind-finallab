using FinalLabInmind.DbContext;
using FinalLabInmind.Interfaces;
using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalLabInmind.Services.TransactionLogService;

public class TransactionLogService : ITransactionLogService
{
    private readonly IAppDbContext _context;
    private readonly IMessagePublisher _messagePublisher;

    public TransactionLogService(IAppDbContext context, IMessagePublisher messagePublisher)
    {
        _context = context;
        _messagePublisher = messagePublisher;
    }

    public async Task<TransactionLog> LogTransactionAsync(TransactionLog transactionLog)
    {
        if (transactionLog == null)
            throw new ArgumentNullException(nameof(transactionLog), "Transaction data is missing.");

        transactionLog.Timestamp = DateTime.UtcNow;
        _context.TransactionLogs.Add(transactionLog);
        await _context.SaveChangesAsync();
        await _messagePublisher.PublishTransactionAsync(transactionLog);
        
        return transactionLog;
    }

    public async Task<List<TransactionLog>> GetTransactionLogsForAccountAsync(long accountId)
    {
        var transactionLogs = await _context.TransactionLogs
            .Where(t => t.AccountId == accountId)
            .ToListAsync();

        foreach (var log in transactionLogs)
        {
            await _messagePublisher.PublishTransactionAsync(log);
        }

        return transactionLogs;
    }

    public IQueryable<TransactionLog> GetTransactionLogs()
    {
        return _context.TransactionLogs;
    }
}
