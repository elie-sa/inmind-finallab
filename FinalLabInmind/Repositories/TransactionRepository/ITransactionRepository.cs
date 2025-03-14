using LoggingMicroservice.Models;

namespace FinalLabInmind.Repositories.TransactionRepository;

public interface ITransactionRepository
{
    Task AddAsync(TransactionLog transaction);
}