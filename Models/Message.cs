using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class Message
{
    public int Id { get; set; }

    public int? MiniGameId { get; set; }

    [MaxLength(120)]
    public string Key { get; set; } = "";

    [MaxLength(160)]
    public string Title { get; set; } = "";

    public string Body { get; set; } = "";

    [MaxLength(80)]
    public string ButtonText { get; set; } = "";

    public int? MediaId { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
