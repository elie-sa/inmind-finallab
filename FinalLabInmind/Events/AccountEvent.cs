using MediatR;

namespace FinalLabInmind;

public record AccountEvent(
    long AccountId,
    long CustomerId,
    DateTime Timestamp,
    string CustomerName
) : INotification;