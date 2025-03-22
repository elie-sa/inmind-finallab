namespace LoggingMicroservice.Models;

public class Account
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public string AccountName { get; set; }
    
    public decimal Balance { get; set; }

    public ICollection<TransactionLog>? Transactions { get; set; } = new List<TransactionLog>();
}