using LoggingMicroservice.Models;

namespace FinalLabInmind.Interfaces;

public interface IMessagePublisher
{
    Task PublishTransactionAsync(TransactionLog transactionEvent);
}