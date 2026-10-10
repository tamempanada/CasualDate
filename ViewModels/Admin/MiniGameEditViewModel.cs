using System.ComponentModel.DataAnnotations;

namespace CasualDate.ViewModels.Admin;

public class MiniGameEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Slug { get; set; } = "";

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = "";

    [MaxLength(400)]
    public string Description { get; set; } = "";

    public int DisplayOrder { get; set; }

    public bool IsEnabled { get; set; } = true;

    [MaxLength(40)]
    public string IconLabel { get; set; } = "";
}
