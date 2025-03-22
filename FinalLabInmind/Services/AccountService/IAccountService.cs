using FinalLabInmind.DTOs;
using LoggingMicroservice.Models;

namespace FinalLabInmind.Services.AccountService;

public interface IAccountService
{
    Task<Account> CreateAccountAsync(Account account);
    Task<Account> GetAccountByIdAsync(long id);
    Task<List<AccountBalanceSummaryDto>> GetAccountBalanceSummaryAsync(long customerId);
    Task<List<TransactionLog>> GetCommonTransactionsAsync(List<long> accountIds);
}