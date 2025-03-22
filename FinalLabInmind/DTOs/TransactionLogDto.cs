using LoggingMicroservice.Models;

namespace FinalLabInmind.DTOs;

public class TransactionLogDto
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public string TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }

    public TransactionLogDto() {}
    
    public TransactionLogDto(TransactionLog transactionLog)
    {
        Id = transactionLog.Id;
        AccountId = transactionLog.AccountId;
        TransactionType = transactionLog.TransactionType;
        Amount = transactionLog.Amount;
        Status = transactionLog.Status;
        Timestamp = transactionLog.Timestamp;
        Details = transactionLog.Details;
    }
}