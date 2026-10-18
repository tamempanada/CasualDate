using System.ComponentModel.DataAnnotations;

namespace CasualDate.ViewModels.Admin;

public class AdminUserCreateViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";

    [Required]
    public string AccountType { get; set; } = "Player";

    public bool IsSuperAdmin { get; set; }
}
