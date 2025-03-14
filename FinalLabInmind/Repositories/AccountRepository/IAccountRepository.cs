using LoggingMicroservice.Models;

namespace FinalLabInmind.Repositories.AccountRepository;

public interface IAccountRepository
{
    Task<Account> GetByIdAsync(long id);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
}