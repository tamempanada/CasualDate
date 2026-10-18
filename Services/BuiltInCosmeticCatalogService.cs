using CasualDate.Data;
using CasualDate.Models;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Services;

public class BuiltInCosmeticCatalogService
{
    private readonly ApplicationDbContext _context;

    public BuiltInCosmeticCatalogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task EnsureSeededAsync()
    {
        var existingIds = await _context.GachaRewards
            .Where(reward => reward.BuiltInCosmeticId != null)
            .Select(reward => reward.BuiltInCosmeticId!)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var missing = BuiltInCosmeticCatalog.Items
            .Where(item => !existingIds.Contains(item.Id, StringComparer.Ordinal))
            .Select((item, index) => new GachaReward
            {
                BuiltInCosmeticId = item.Id,
                RewardType = GachaRewardType.Cosmetic,
                Rarity = GachaRarity.Legendary,
                Name = item.Name,
                Description = item.Description,
                IsActive = true,
                DisplayOrder = 10_000 + index,
                CreatedAt = now,
                UpdatedAt = now
            })
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        _context.GachaRewards.AddRange(missing);
        await _context.SaveChangesAsync();
    }
}
