using System.ComponentModel.DataAnnotations;

namespace CasualDate.ViewModels.Admin;

public class TokenRewardsEditViewModel
{
    public List<TokenRewardEditItemViewModel> Rewards { get; set; } = [];
}

public class TokenRewardEditItemViewModel
{
    public int Id { get; set; }

    public string EventKey { get; set; } = "";

    public string DisplayName { get; set; } = "";

    public string GroupName { get; set; } = "General";

    [Range(0, 10000, ErrorMessage = "Reward must be between 0 and 10,000 tokens.")]
    public int TokenAmount { get; set; }

    [Range(1, 100000, ErrorMessage = "Milestone interval must be between 1 and 100,000.")]
    public int TriggerInterval { get; set; } = 1;

    public bool IsEnabled { get; set; }
}
