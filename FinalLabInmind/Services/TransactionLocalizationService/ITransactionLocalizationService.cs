namespace FinalLabInmind.Services.TransactionLocalizationService;

public interface ITransactionLocalizationService
{
    Task<string> GetLocalizedTransactionNotificationAsync(long transactionId, string language);
}