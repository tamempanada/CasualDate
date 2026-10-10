using System.ComponentModel.DataAnnotations;

namespace CasualDate.ViewModels.Admin;

public class DateOptionEditViewModel
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string? Name { get; set; } = "";

    [MaxLength(400)]
    public string? Description { get; set; } = "";

    public int Score { get; set; } = 20;

    public int? MediaId { get; set; }

    public string? ImagePath { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsEnabled { get; set; } = true;
}
