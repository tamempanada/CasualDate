namespace CasualDate.ViewModels.Player;

public class PlayerWalletViewModel
{
    public string DisplayName { get; set; } = "Player";

    public int TokenBalance { get; set; }

    public List<PlayerWalletTransactionViewModel> Transactions { get; set; } = [];
}

public class PlayerWalletTransactionViewModel
{
    public int Amount { get; set; }

    public int BalanceAfter { get; set; }

    public string TransactionType { get; set; } = "";

    public string Description { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}
