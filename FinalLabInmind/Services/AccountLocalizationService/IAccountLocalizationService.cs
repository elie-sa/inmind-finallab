namespace FinalLabInmind.Services.AccountLocalizationService;

public interface IAccountLocalizationService
{
    Task<string> GetLocalizedAccountDetailsAsync(long accountId, string language);
}