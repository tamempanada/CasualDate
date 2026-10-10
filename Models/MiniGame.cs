using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class MiniGame
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string Slug { get; set; } = "";

    [MaxLength(120)]
    public string Name { get; set; } = "";

    [MaxLength(400)]
    public string Description { get; set; } = "";

    public int DisplayOrder { get; set; }

    public bool IsEnabled { get; set; } = true;

    [MaxLength(40)]
    public string IconLabel { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
