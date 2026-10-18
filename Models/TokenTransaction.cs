using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class TokenTransaction
{
    public int Id { get; set; }

    public int PlayerProfileId { get; set; }

    public int Amount { get; set; }

    public int BalanceAfter { get; set; }

    [MaxLength(80)]
    public string TransactionType { get; set; } = "";

    [MaxLength(120)]
    public string ReferenceKey { get; set; } = "";

    [MaxLength(300)]
    public string Description { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
