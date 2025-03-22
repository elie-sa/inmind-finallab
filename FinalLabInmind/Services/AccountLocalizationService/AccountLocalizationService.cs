using System.Globalization;
using FinalLabInmind.DbContext;
using FinalLabInmind.Resources;
using Microsoft.Extensions.Localization;

namespace FinalLabInmind.Services.AccountLocalizationService;

public class AccountLocalizationService : IAccountLocalizationService
{
    private readonly IAppDbContext _context;
    private readonly IStringLocalizer<AccountDetails> _localizer;

    public AccountLocalizationService(IAppDbContext context, IStringLocalizer<AccountDetails> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<string> GetLocalizedAccountDetailsAsync(long accountId, string language)
    {
        var account = await _context.Accounts.FindAsync(accountId);
       
        var culture = new CultureInfo(language);
        Thread.CurrentThread.CurrentUICulture = culture;

        string resourceKey = account.AccountName;
        var localizedDetails = _localizer[resourceKey];

        if (string.IsNullOrEmpty(localizedDetails))
        {
            localizedDetails = _localizer["Account_Default_Details"];
        }

        return localizedDetails;
    }
}