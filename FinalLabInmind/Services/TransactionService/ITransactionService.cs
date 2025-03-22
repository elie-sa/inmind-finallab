namespace FinalLabInmind.Services.TransactionService;

public interface ITransactionService
{
    Task<string> TransferFundsAsync(long fromAccountId, long toAccountId, decimal amount);
}