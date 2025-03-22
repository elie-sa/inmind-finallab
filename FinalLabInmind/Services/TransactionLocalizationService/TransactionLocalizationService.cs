using System.Globalization;
using FinalLabInmind.DbContext;
using FinalLabInmind.Resources;
using Microsoft.Extensions.Localization;

namespace FinalLabInmind.Services.TransactionLocalizationService;

public class TransactionLocalizationService : ITransactionLocalizationService
{
    private readonly IAppDbContext _context;
    private readonly IStringLocalizer<TransactionDetails> _localizer;

    public TransactionLocalizationService(IAppDbContext context, IStringLocalizer<TransactionDetails> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<string> GetLocalizedTransactionNotificationAsync(long transactionId, string language)
    {
        var transaction = await _context.TransactionLogs.FindAsync(transactionId);
        if (transaction == null)
        {
            throw new System.ArgumentException($"Transaction with ID {transactionId} not found.");
        }

        var culture = new CultureInfo(language);
        Thread.CurrentThread.CurrentUICulture = culture;
        
        string resourceKey = transaction.TransactionType;
        var localizedNotification = _localizer[resourceKey];
        Console.WriteLine(localizedNotification);
        if (string.IsNullOrWhiteSpace(localizedNotification))
        {
            localizedNotification = _localizer["Transaction_Default_Details"];
        }

        return localizedNotification;
    }
}