namespace Accounting;

public class AccountingTask
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Assignee { get; set; }
    public bool IsComplete { get; set; }
    public decimal Cost { get; set; }
    public decimal Payout { get; set; }
}