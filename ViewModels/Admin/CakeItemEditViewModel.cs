using System.ComponentModel.DataAnnotations;
using CasualDate.Models;

namespace CasualDate.ViewModels.Admin;

public class CakeItemEditViewModel
{
    public int Id { get; set; }

    public CakeItemType ItemType { get; set; } = CakeItemType.Base;

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = "";

    [MaxLength(400)]
    public string Description { get; set; } = "";

    public int Score { get; set; } = 20;

    public int? MediaId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsEnabled { get; set; } = true;
}
