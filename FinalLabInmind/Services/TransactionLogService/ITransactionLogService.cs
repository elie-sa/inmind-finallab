using LoggingMicroservice.Models;

namespace FinalLabInmind.Services.TransactionLogService;

public interface ITransactionLogService
{
    Task<TransactionLog> LogTransactionAsync(TransactionLog transactionLog);
    Task<List<TransactionLog>> GetTransactionLogsForAccountAsync(long accountId);
    IQueryable<TransactionLog> GetTransactionLogs();
}