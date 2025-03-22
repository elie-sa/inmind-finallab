using FinalLabInmind.DTOs;
using LoggingMicroservice.Models;

namespace FinalLabInmind.Services.TransactionLogService;

public interface ITransactionLogService
{
    Task<TransactionLogDto> LogTransactionAsync(TransactionLogDto transactionLogDto);
    Task<List<TransactionLogDto>> GetTransactionLogsForAccountAsync(long accountId);
    IQueryable<TransactionLogDto> GetTransactionLogs();
    
}