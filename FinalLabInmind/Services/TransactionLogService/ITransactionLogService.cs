using FinalLabInmind.DTOs;
using LoggingMicroservice.Models;

namespace FinalLabInmind.Services.TransactionLogService;

public interface ITransactionLogService
{
    Task<TransactionLogDto> LogTransactionAsync(TransactionLogDto transactionLogDto);
    Task<List<TransactionLogDto>> GetTransactionLogsForAccountAsync(long accountId);
    IQueryable<TransactionLogDto> GetTransactionLogs();
    Task<List<TransactionLog>> GetCommonTransactionsAsync(List<long> accountIds);
    Task<List<AccountBalanceSummaryDto>> GetAccountBalanceSummaryAsync(long customerId);
    Task<Account> CreateAccountAsync(Account account);
    Task<Account> GetAccountByIdAsync(long id);
}