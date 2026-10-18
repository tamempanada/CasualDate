using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class GachaPullBatch
{
    public int Id { get; set; }

    public int PlayerProfileId { get; set; }

    [MaxLength(80)]
    public string RequestToken { get; set; } = "";

    [MaxLength(80)]
    public string BatchToken { get; set; } = "";

    public int PullCount { get; set; }

    public int TokenCost { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
