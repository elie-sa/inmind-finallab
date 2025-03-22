using MediatR;

namespace FinalLabInmind.Models;

public class AccountEvent: INotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long AccountId { get; set; } 
    public string EventType { get; set; }
    public string Details { get; set; }
    public DateTime Timestamp { get; set; }
}