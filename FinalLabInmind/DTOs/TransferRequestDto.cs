namespace FinalLabInmind.DTOs;

public class TransferRequestDto
{
    public long FromAccountId { get; set; }
    public long ToAccountId { get; set; }
    public decimal Amount { get; set; }
}