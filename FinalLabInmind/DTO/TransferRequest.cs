namespace FinalLabInmind.DTO;

public class TransferRequest
{
    public long FromAccountId { get; set; }
    public long ToAccountId { get; set; }
    public decimal Amount { get; set; }
}
