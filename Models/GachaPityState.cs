namespace CasualDate.Models;

public class GachaPityState
{
    public int Id { get; set; }

    public int PlayerProfileId { get; set; }

    public int LegendaryPityCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
