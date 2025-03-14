
using FinalLabInmind.DbContext;
using LoggingMicroservice.Models;

namespace FinalLabInmind.Repositories.AccountRepository;

public class AccountRepository : IAccountRepository
{
    private readonly IAppDbContext _context;

    public AccountRepository(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Account> GetByIdAsync(long id)
    {
        return await _context.Accounts.FindAsync(id);
    }

    public async Task AddAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
    }
}