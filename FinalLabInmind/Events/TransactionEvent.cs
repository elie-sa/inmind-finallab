using MediatR;

namespace FinalLabInmind;

public record TransactionEvent(
    long TransactionId,
    long AccountId,
    string EventType,
    decimal Amount,
    DateTime Timestamp,
    string Status,
    string Details
) : INotification;