using CasualDate.Data;
using CasualDate.Models;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Services;

public class PlayerCosmeticService
{
    private readonly ApplicationDbContext _context;
    private readonly DatabaseSchemaService _schemaService;
    private readonly PlayerProfileService _playerProfileService;
    private readonly BuiltInCosmeticCatalogService _catalogService;

    public PlayerCosmeticService(
        ApplicationDbContext context,
        DatabaseSchemaService schemaService,
        PlayerProfileService playerProfileService,
        BuiltInCosmeticCatalogService catalogService)
    {
        _context = context;
        _schemaService = schemaService;
        _playerProfileService = playerProfileService;
        _catalogService = catalogService;
    }

    public async Task<PlayerCosmeticStateDto> GetStateAsync()
    {
        await EnsureSchemaAsync();
        await _catalogService.EnsureSeededAsync();
        var player = await _playerProfileService.GetOrCreateAsync();
        var owned = await GetOwnedRewardsAsync(player.Id);
        var ownedCosmeticIds = owned
            .Where(item => !string.IsNullOrWhiteSpace(item.Reward!.BuiltInCosmeticId))
            .Select(item => item.Reward!.BuiltInCosmeticId!)
            .ToHashSet(StringComparer.Ordinal);

        var cosmetics = BuiltInCosmeticCatalog.Items.Select(item =>
        {
            var reward = owned.FirstOrDefault(ownedReward => ownedReward.Reward?.BuiltInCosmeticId == item.Id)?.Reward;
            return new PlayerCosmeticItemDto(
                item.Id,
                item.Name,
                item.Category.ToString(),
                item.Description,
                item.StyleKey,
                ownedCosmeticIds.Contains(item.Id),
                IsEquipped(player, item),
                reward?.Id,
                reward?.Media?.FilePath);
        }).ToList();

        var companions = owned
            .Where(item => item.Reward?.RewardType == GachaRewardType.Doll)
            .OrderBy(item => item.Reward!.Name)
            .Select(item => new PlayerCompanionDto(
                item.Reward!.Id,
                item.Reward.Name,
                item.Reward.Description,
                item.Reward.Rarity.ToString(),
                item.Reward.Media?.FilePath,
                player.EquippedCompanionRewardId == item.Reward.Id))
            .ToList();

        var tracks = await _context.UploadedMedia
            .Where(media => media.MediaType == MediaType.Audio)
            .OrderBy(media => media.DisplayName)
            .Select(media => new PlayerMusicTrackDto(media.Id, media.DisplayName, player.SelectedMusicMediaId == media.Id))
            .ToListAsync();

        return new PlayerCosmeticStateDto(
            cosmetics,
            companions,
            tracks,
            new PlayerCosmeticPreferencesDto(
                player.EquippedThemeId,
                player.EquippedMusicSkinId,
                player.EquippedEffectId,
                player.EquippedCompanionRewardId,
                player.SelectedMusicMediaId));
    }

    public async Task<PlayerCompanionDto?> GetEquippedCompanionAsync()
    {
        await EnsureSchemaAsync();
        var player = await _playerProfileService.GetOrCreateAsync();
        if (!player.EquippedCompanionRewardId.HasValue)
        {
            return null;
        }

        var owned = await _context.GachaCollectionItems
            .Include(item => item.Reward)
            .ThenInclude(reward => reward!.Media)
            .FirstOrDefaultAsync(item => item.PlayerProfileId == player.Id
                && item.GachaRewardId == player.EquippedCompanionRewardId.Value
                && item.Reward != null
                && item.Reward.RewardType == GachaRewardType.Doll);
        return owned?.Reward is null
            ? null
            : new PlayerCompanionDto(owned.Reward.Id, owned.Reward.Name, owned.Reward.Description, owned.Reward.Rarity.ToString(), owned.Reward.Media?.FilePath, true);
    }

    public async Task<PlayerCosmeticPreferencesDto> EquipCosmeticAsync(string category, string? cosmeticId)
    {
        await EnsureSchemaAsync();
        await _catalogService.EnsureSeededAsync();
        var player = await _playerProfileService.GetOrCreateAsync();
        var normalizedCategory = category?.Trim().ToLowerInvariant();
        var expectedCategory = normalizedCategory switch
        {
            "theme" => CosmeticCategory.Theme,
            "music" or "musicskin" => CosmeticCategory.MusicSkin,
            "effect" => CosmeticCategory.Effect,
            _ => throw new InvalidOperationException("Unknown cosmetic category.")
        };

        if (!string.IsNullOrWhiteSpace(cosmeticId))
        {
            var cosmetic = BuiltInCosmeticCatalog.Find(cosmeticId);
            if (cosmetic is null || cosmetic.Category != expectedCategory)
            {
                throw new InvalidOperationException("That cosmetic is not available.");
            }

            var isOwned = await _context.GachaCollectionItems
                .AnyAsync(item => item.PlayerProfileId == player.Id
                    && item.Reward != null
                    && item.Reward.BuiltInCosmeticId == cosmetic.Id);
            if (!isOwned)
            {
                throw new InvalidOperationException("Collect that cosmetic from Gacha before equipping it.");
            }
        }

        switch (expectedCategory)
        {
            case CosmeticCategory.Theme:
                player.EquippedThemeId = cosmeticId ?? BuiltInCosmeticCatalog.DefaultThemeId;
                break;
            case CosmeticCategory.MusicSkin:
                player.EquippedMusicSkinId = cosmeticId ?? BuiltInCosmeticCatalog.DefaultMusicSkinId;
                break;
            case CosmeticCategory.Effect:
                player.EquippedEffectId = cosmeticId ?? BuiltInCosmeticCatalog.NoEffectId;
                break;
        }

        player.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToPreferences(player);
    }

    public async Task<PlayerCosmeticPreferencesDto> EquipCompanionAsync(int? rewardId)
    {
        await EnsureSchemaAsync();
        var player = await _playerProfileService.GetOrCreateAsync();

        if (rewardId.HasValue)
        {
            var isOwnedDoll = await _context.GachaCollectionItems.AnyAsync(item =>
                item.PlayerProfileId == player.Id
                && item.GachaRewardId == rewardId.Value
                && item.Reward != null
                && item.Reward.RewardType == GachaRewardType.Doll);
            if (!isOwnedDoll)
            {
                throw new InvalidOperationException("Only dolls in your collection can become companions.");
            }
        }

        player.EquippedCompanionRewardId = rewardId;
        player.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToPreferences(player);
    }

    public async Task<PlayerCosmeticPreferencesDto> SelectTrackAsync(int? mediaId)
    {
        await EnsureSchemaAsync();
        var player = await _playerProfileService.GetOrCreateAsync();
        if (mediaId.HasValue && !await _context.UploadedMedia.AnyAsync(media => media.Id == mediaId.Value && media.MediaType == MediaType.Audio))
        {
            throw new InvalidOperationException("That music track is not available.");
        }

        player.SelectedMusicMediaId = mediaId;
        player.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToPreferences(player);
    }

    private async Task<List<GachaCollectionItem>> GetOwnedRewardsAsync(int playerId) => await _context.GachaCollectionItems
        .Include(item => item.Reward)
        .ThenInclude(reward => reward!.Media)
        .Where(item => item.PlayerProfileId == playerId && item.Reward != null)
        .ToListAsync();

    private async Task EnsureSchemaAsync()
    {
        var required = new[]
        {
            "EquippedThemeId", "EquippedMusicSkinId", "EquippedEffectId", "EquippedCompanionRewardId", "SelectedMusicMediaId"
        };
        foreach (var column in required)
        {
            if (!await _schemaService.ColumnExistsAsync("PlayerProfiles", column))
            {
                throw new PlayerDataUnavailableException("Cosmetic preferences are not available yet. Restart the app or run database migrations.");
            }
        }
    }

    private static bool IsEquipped(PlayerProfile player, BuiltInCosmetic cosmetic) => cosmetic.Category switch
    {
        CosmeticCategory.Theme => player.EquippedThemeId == cosmetic.Id,
        CosmeticCategory.MusicSkin => player.EquippedMusicSkinId == cosmetic.Id,
        CosmeticCategory.Effect => player.EquippedEffectId == cosmetic.Id,
        _ => false
    };

    private static PlayerCosmeticPreferencesDto ToPreferences(PlayerProfile player) => new(
        player.EquippedThemeId,
        player.EquippedMusicSkinId,
        player.EquippedEffectId,
        player.EquippedCompanionRewardId,
        player.SelectedMusicMediaId);
}

public sealed record PlayerCosmeticItemDto(string Id, string Name, string Category, string Description, string StyleKey, bool IsOwned, bool IsEquipped, int? RewardId, string? ImagePath);
public sealed record PlayerCompanionDto(int RewardId, string Name, string Description, string Rarity, string? ImagePath, bool IsEquipped);
public sealed record PlayerMusicTrackDto(int Id, string Name, bool IsSelected);
public sealed record PlayerCosmeticPreferencesDto(string ThemeId, string MusicSkinId, string EffectId, int? CompanionRewardId, int? SelectedMusicMediaId);
public sealed record PlayerCosmeticStateDto(IReadOnlyList<PlayerCosmeticItemDto> Cosmetics, IReadOnlyList<PlayerCompanionDto> Companions, IReadOnlyList<PlayerMusicTrackDto> Tracks, PlayerCosmeticPreferencesDto Preferences);
