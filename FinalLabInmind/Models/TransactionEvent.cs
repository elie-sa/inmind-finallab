using MediatR;

namespace FinalLabInmind.Models;

public class TransactionEvent: INotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long TransactionId { get; set; }  // Transaction reference
    public string EventType { get; set; }
    public string Details { get; set; }
    public DateTime Timestamp { get; set; }
}