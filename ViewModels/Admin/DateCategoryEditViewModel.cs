using System.ComponentModel.DataAnnotations;

namespace CasualDate.ViewModels.Admin;

public class DateCategoryEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = "";

    [MaxLength(400)]
    public string? Description { get; set; } = "";

    public int DisplayOrder { get; set; }

    public bool IsEnabled { get; set; } = true;

    public List<DateOptionEditViewModel> Options { get; set; } = new();
}
