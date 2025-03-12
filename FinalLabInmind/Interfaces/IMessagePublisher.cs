using LoggingMicroservice.Models;

namespace FinalLabInmind.Interfaces;

public interface IMessagePublisher
{
    Task PublishTransactionAsync(TransactionLog transactionEvent);
    Task PublishLogAsync(string message);
}