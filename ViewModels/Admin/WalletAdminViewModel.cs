namespace CasualDate.ViewModels.Admin;

using System.ComponentModel.DataAnnotations;

public class WalletAdminViewModel
{
    public List<WalletPlayerViewModel> Players { get; set; } = [];

    public List<WalletTransactionViewModel> Transactions { get; set; } = [];

    public WalletAdjustmentViewModel Adjustment { get; set; } = new();
}

public class WalletPlayerViewModel
{
    public int PlayerProfileId { get; set; }

    public string Email { get; set; } = "";

    public string DisplayName { get; set; } = "";

    public int TokenBalance { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class WalletTransactionViewModel
{
    public string PlayerEmail { get; set; } = "";

    public int Amount { get; set; }

    public int BalanceAfter { get; set; }

    public string TransactionType { get; set; } = "";

    public string ReferenceKey { get; set; } = "";

    public string Description { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}

public class WalletAdjustmentViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Choose a player.")]
    public int PlayerProfileId { get; set; }

    [Range(-10000, 10000, ErrorMessage = "Adjustment must be between -10,000 and 10,000 tokens.")]
    public int Amount { get; set; }

    [Required]
    [StringLength(180)]
    public string Reason { get; set; } = "";
}
