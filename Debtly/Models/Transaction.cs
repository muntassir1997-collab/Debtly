namespace Debtly.Models;

public class Transaction
{
    public int Id { get; set; }

    public int PersonId { get; set; }

    public decimal Amount { get; set; }

    public bool IsFromPerson { get; set; }

    public string Note { get; set; } = string.Empty;

    public DateTime Date { get; set; }
}