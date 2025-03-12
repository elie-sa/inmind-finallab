using System.ComponentModel.DataAnnotations;

namespace LoggingMicroservice.Models;

public class TransactionLog
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long AccountId { get; set; }

    [Required]
    public string TransactionType { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    [Required]
    public string Status { get; set; }

    public string Details { get; set; }
    
    public Account? Account { get; set; }

}