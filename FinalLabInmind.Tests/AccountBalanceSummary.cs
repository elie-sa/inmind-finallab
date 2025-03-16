namespace FinalLabInmind.Tests;

public class AccountBalanceSummary
{
    public long AccountId { get; set; }
    public string AccountName { get; set; }
    public decimal TotalDeposits { get; set; }
    public decimal TotalWithdrawals { get; set; }
    public decimal CurrentBalance { get; set; }
}