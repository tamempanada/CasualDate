using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class CakeItem
{
    public int Id { get; set; }

    public CakeItemType ItemType { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = "";

    [MaxLength(400)]
    public string Description { get; set; } = "";

    public int Score { get; set; }

    public int? MediaId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
