using CasualDate.Data;
using CasualDate.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Services;

public class GachaService
{
    public const int SinglePullCost = 5;
    public const int TenPullCost = 50;
    public const int LegendaryPityThreshold = 90;

    private static readonly Dictionary<GachaRarity, int> RarityWeights = new()
    {
        [GachaRarity.Common] = 60,
        [GachaRarity.Rare] = 28,
        [GachaRarity.Epic] = 10,
        [GachaRarity.Legendary] = 2
    };

    private static readonly Dictionary<GachaRarity, int> RefundValues = new()
    {
        [GachaRarity.Common] = 1,
        [GachaRarity.Rare] = 2,
        [GachaRarity.Epic] = 5,
        [GachaRarity.Legendary] = 10
    };

    private readonly ApplicationDbContext _context;
    private readonly DatabaseSchemaService _schemaService;
    private readonly PlayerProfileService _playerProfileService;
    private readonly TokenWalletService _walletService;
    private readonly BuiltInCosmeticCatalogService _catalogService;
    private readonly Random _random = Random.Shared;

    public GachaService(
        ApplicationDbContext context,
        DatabaseSchemaService schemaService,
        PlayerProfileService playerProfileService,
        TokenWalletService walletService,
        BuiltInCosmeticCatalogService catalogService)
    {
        _context = context;
        _schemaService = schemaService;
        _playerProfileService = playerProfileService;
        _walletService = walletService;
        _catalogService = catalogService;
    }

    public async Task<GachaEconomyStateDto> GetEconomyStateAsync()
    {
        await EnsureEconomySchemaAsync();
        await _catalogService.EnsureSeededAsync();
        var player = await _playerProfileService.GetOrCreateAsync();
        var pity = await GetOrCreatePityStateAsync(player.Id);
        return new GachaEconomyStateDto(
            player.TokenBalance,
            pity.LegendaryPityCount,
            LegendaryPityThreshold,
            SinglePullCost,
            TenPullCost,
            RefundValues.ToDictionary(item => item.Key.ToString(), item => item.Value));
    }

    public async Task<GachaPullResult> PullAsync(int pullCount, string requestToken)
    {
        await EnsureEconomySchemaAsync();
        await _catalogService.EnsureSeededAsync();

        if (pullCount is not 1 and not 10)
        {
            return GachaPullResult.Invalid("Choose either 1 pull or 10 pulls.");
        }

        if (string.IsNullOrWhiteSpace(requestToken) || requestToken.Length > 80)
        {
            return GachaPullResult.Invalid("Missing pull request token.");
        }

        var player = await _playerProfileService.GetOrCreateAsync();
        var playerId = player.Id;
        var pullRequestToken = requestToken.Trim();
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var existingBatch = await _context.GachaPullBatches
            .FirstOrDefaultAsync(batch => batch.PlayerProfileId == playerId && batch.RequestToken == pullRequestToken);
        if (existingBatch is not null)
        {
            var existingPulls = await LoadPullsForBatchAsync(existingBatch.Id);
            await transaction.CommitAsync();
            return GachaPullResult.Success(existingBatch.BatchToken, existingPulls, await GetBalanceAsync(playerId), await GetPityCountAsync(playerId));
        }

        var alreadyReservedCosmeticIds = await _context.GachaPulls
            .Where(pull => pull.PlayerProfileId == playerId
                && pull.Status == GachaPullStatus.Pending
                && pull.Reward != null
                && pull.Reward.BuiltInCosmeticId != null)
            .Select(pull => pull.GachaRewardId)
            .ToListAsync();
        var ownedCosmeticIds = await _context.GachaCollectionItems
            .Where(item => item.PlayerProfileId == playerId
                && item.Reward != null
                && item.Reward.BuiltInCosmeticId != null)
            .Select(item => item.GachaRewardId)
            .ToListAsync();
        var unavailableCosmeticRewardIds = alreadyReservedCosmeticIds
            .Concat(ownedCosmeticIds)
            .Distinct()
            .ToList();
        var rewards = await _context.GachaRewards
            .Include(reward => reward.Media)
            .Where(reward => reward.IsActive
                && (reward.BuiltInCosmeticId == null || !unavailableCosmeticRewardIds.Contains(reward.Id)))
            .ToListAsync();
        var validation = ValidatePullPool(rewards, pullCount, await GetPityCountAsync(playerId));
        if (validation is not null)
        {
            return validation;
        }

        var cost = pullCount == 10 ? TenPullCost : SinglePullCost;
        var batch = new GachaPullBatch
        {
            PlayerProfileId = playerId,
            RequestToken = pullRequestToken,
            BatchToken = Guid.NewGuid().ToString("N"),
            PullCount = pullCount,
            TokenCost = cost
        };
        _context.GachaPullBatches.Add(batch);
        await _context.SaveChangesAsync();

        var spend = await _walletService.TrySpendAsync(playerId, cost, "GachaPull", batch.BatchToken, pullCount == 10 ? "Girlfriend Gacha 10-pull." : "Girlfriend Gacha single pull.");
        if (!spend.Succeeded)
        {
            await transaction.RollbackAsync();
            return GachaPullResult.InsufficientTokens(spend.Balance, cost);
        }

        var pity = await GetOrCreatePityStateAsync(playerId);
        var generated = GenerateRewards(rewards, pullCount, pity.LegendaryPityCount);
        pity.LegendaryPityCount = generated.PityAfter;
        pity.UpdatedAt = DateTime.UtcNow;

        var pulls = generated.Rewards.Select(result => new GachaPull
        {
            PlayerProfileId = playerId,
            GachaPullBatchId = batch.Id,
            GachaRewardId = result.Reward.Id,
            PullToken = Guid.NewGuid().ToString("N"),
            IsPityGuaranteed = result.IsPityGuaranteed,
            IsTenPullGuaranteed = result.IsTenPullGuaranteed,
            RefundAmount = GetRefundValue(result.Reward.Rarity)
        }).ToList();

        _context.GachaPulls.AddRange(pulls);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        var dtos = pulls.Select((pull, index) => ToPullDto(pull, generated.Rewards[index].Reward)).ToList();
        return GachaPullResult.Success(batch.BatchToken, dtos, spend.Balance, pity.LegendaryPityCount);
    }

    public async Task<GachaDecisionResult> KeepAsync(string pullToken)
    {
        return await ResolvePullAsync(pullToken, GachaPullStatus.Kept);
    }

    public async Task<GachaDecisionResult> TrashAsync(string pullToken)
    {
        return await ResolvePullAsync(pullToken, GachaPullStatus.Trashed);
    }

    public async Task<IReadOnlyList<GachaRewardDto>> GetCollectionAsync()
    {
        await EnsureEconomySchemaAsync();
        await _catalogService.EnsureSeededAsync();
        var player = await _playerProfileService.GetOrCreateAsync();
        var items = await _context.GachaCollectionItems
            .Include(item => item.Reward)
            .ThenInclude(reward => reward!.Media)
            .Where(item => item.PlayerProfileId == player.Id && item.Reward != null)
            .OrderByDescending(item => item.LastKeptAt)
            .ToListAsync();

        return items
            .Where(item => item.Reward is not null)
            .Select(ToCollectionDto)
            .ToList();
    }

    public async Task<GachaPullResult?> GetLatestPendingPullsAsync()
    {
        await EnsureEconomySchemaAsync();
        await _catalogService.EnsureSeededAsync();
        var player = await _playerProfileService.GetOrCreateAsync();
        var playerId = player.Id;
        var batch = await _context.GachaPullBatches
            .Where(item => item.PlayerProfileId == playerId
                && _context.GachaPulls.Any(pull => pull.GachaPullBatchId == item.Id && pull.Status == GachaPullStatus.Pending))
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync();
        if (batch is null)
        {
            return null;
        }

        var pulls = await LoadPullsForBatchAsync(batch.Id, pendingOnly: true);
        return pulls.Count == 0
            ? null
            : GachaPullResult.Success(batch.BatchToken, pulls, await GetBalanceAsync(playerId), await GetPityCountAsync(playerId));
    }

    private async Task<GachaDecisionResult> ResolvePullAsync(string pullToken, GachaPullStatus status)
    {
        await EnsureEconomySchemaAsync();

        if (string.IsNullOrWhiteSpace(pullToken))
        {
            return GachaDecisionResult.Invalid("That pull could not be found.");
        }

        var player = await _playerProfileService.GetOrCreateAsync();
        var playerId = player.Id;
        var activePullToken = pullToken.Trim();
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var pull = await _context.GachaPulls
            .Include(item => item.Reward)
            .ThenInclude(reward => reward!.Media)
            .FirstOrDefaultAsync(item => item.PullToken == activePullToken && item.PlayerProfileId == playerId);

        if (pull is null)
        {
            return GachaDecisionResult.Invalid("That pull could not be found.");
        }

        if (pull.Status != GachaPullStatus.Pending)
        {
            return GachaDecisionResult.Invalid("That reward was already resolved.");
        }

        pull.Status = status;
        pull.ResolvedAt = DateTime.UtcNow;
        var refundAmount = 0;
        var balance = await GetBalanceAsync(playerId);

        if (status == GachaPullStatus.Kept)
        {
            var collectionItem = await _context.GachaCollectionItems
                .FirstOrDefaultAsync(item => item.PlayerProfileId == playerId && item.GachaRewardId == pull.GachaRewardId);
            if (collectionItem is null)
            {
                collectionItem = new GachaCollectionItem
                {
                    PlayerProfileId = playerId,
                    GachaRewardId = pull.GachaRewardId
                };
                ApplySnapshot(collectionItem, pull.Reward!);
                _context.GachaCollectionItems.Add(collectionItem);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(collectionItem.SnapshotName))
                {
                    ApplySnapshot(collectionItem, pull.Reward!);
                }

                collectionItem.Quantity += 1;
                collectionItem.LastKeptAt = DateTime.UtcNow;
            }
        }
        else
        {
            refundAmount = pull.RefundAmount;
            if (refundAmount > 0)
            {
                balance = await _walletService.AddBalanceAtomicallyAsync(playerId, refundAmount, "GachaDiscardRefund", pull.PullToken, $"Discard refund for {pull.Reward!.Name}.");
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return GachaDecisionResult.Success(ToDto(pull.Reward!, status == GachaPullStatus.Kept ? 1 : 0), status, refundAmount, balance);
    }

    private GachaPullResult? ValidatePullPool(IReadOnlyList<GachaReward> rewards, int pullCount, int pityCount)
    {
        if (rewards.Count == 0)
        {
            return GachaPullResult.Invalid("No active Gacha rewards are available yet.");
        }

        if (rewards.All(reward => !string.IsNullOrWhiteSpace(reward.BuiltInCosmeticId)) && rewards.Count < pullCount)
        {
            return GachaPullResult.Invalid("There are not enough unowned cosmetics available for that pull.");
        }

        if (pullCount == 10 && rewards.All(reward => reward.Rarity < GachaRarity.Epic))
        {
            return GachaPullResult.Invalid("A 10-pull needs at least one active Epic or Legendary reward.");
        }

        if (pityCount + pullCount >= LegendaryPityThreshold && rewards.All(reward => reward.Rarity != GachaRarity.Legendary))
        {
            return GachaPullResult.Invalid("Legendary pity needs an active Legendary reward before pulling.");
        }

        return null;
    }

    private async Task EnsureEconomySchemaAsync()
    {
        await _context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS `GachaPullBatches` (
                `Id` int NOT NULL AUTO_INCREMENT,
                `PlayerProfileId` int NOT NULL,
                `RequestToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
                `BatchToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
                `PullCount` int NOT NULL,
                `TokenCost` int NOT NULL,
                `CreatedAt` datetime(6) NOT NULL,
                PRIMARY KEY (`Id`),
                UNIQUE KEY `IX_GachaPullBatches_BatchToken` (`BatchToken`),
                UNIQUE KEY `IX_GachaPullBatches_PlayerProfileId_RequestToken` (`PlayerProfileId`, `RequestToken`)
            ) CHARACTER SET=utf8mb4;
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS `GachaPityStates` (
                `Id` int NOT NULL AUTO_INCREMENT,
                `PlayerProfileId` int NOT NULL,
                `LegendaryPityCount` int NOT NULL,
                `CreatedAt` datetime(6) NOT NULL,
                `UpdatedAt` datetime(6) NOT NULL,
                PRIMARY KEY (`Id`),
                UNIQUE KEY `IX_GachaPityStates_PlayerProfileId` (`PlayerProfileId`)
            ) CHARACTER SET=utf8mb4;
            """);

        if (!await _schemaService.ColumnExistsAsync("GachaPulls", "GachaPullBatchId"))
        {
            await _context.Database.ExecuteSqlRawAsync("ALTER TABLE `GachaPulls` ADD COLUMN `GachaPullBatchId` int NULL;");
        }

        if (!await _schemaService.ColumnExistsAsync("GachaPulls", "IsPityGuaranteed"))
        {
            await _context.Database.ExecuteSqlRawAsync("ALTER TABLE `GachaPulls` ADD COLUMN `IsPityGuaranteed` tinyint(1) NOT NULL DEFAULT FALSE;");
        }

        if (!await _schemaService.ColumnExistsAsync("GachaPulls", "IsTenPullGuaranteed"))
        {
            await _context.Database.ExecuteSqlRawAsync("ALTER TABLE `GachaPulls` ADD COLUMN `IsTenPullGuaranteed` tinyint(1) NOT NULL DEFAULT FALSE;");
        }

        if (!await _schemaService.ColumnExistsAsync("GachaPulls", "RefundAmount"))
        {
            await _context.Database.ExecuteSqlRawAsync("ALTER TABLE `GachaPulls` ADD COLUMN `RefundAmount` int NOT NULL DEFAULT 0;");
        }

        if (!await _schemaService.ColumnExistsAsync("GachaRewards", "BuiltInCosmeticId"))
        {
            await _context.Database.ExecuteSqlRawAsync("ALTER TABLE `GachaRewards` ADD COLUMN `BuiltInCosmeticId` varchar(80) CHARACTER SET utf8mb4 NULL;");
        }

        await EnsureCollectionSnapshotColumnAsync("SnapshotName");
        await EnsureCollectionSnapshotColumnAsync("SnapshotDescription");
        await EnsureCollectionSnapshotColumnAsync("SnapshotRewardType");
        await EnsureCollectionSnapshotColumnAsync("SnapshotRarity");
        await EnsureCollectionSnapshotColumnAsync("SnapshotImagePath");
        await EnsureCollectionSnapshotColumnAsync("SnapshotLetterTitle");
        await EnsureCollectionSnapshotColumnAsync("SnapshotLetterContent");
        await BackfillCollectionSnapshotsAsync();
    }

    private async Task EnsureCollectionSnapshotColumnAsync(string columnName)
    {
        if (!await _schemaService.ColumnExistsAsync("GachaCollectionItems", columnName))
        {
            var sql = columnName switch
            {
                "SnapshotName" => "ALTER TABLE `GachaCollectionItems` ADD COLUMN `SnapshotName` varchar(120) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';",
                "SnapshotDescription" => "ALTER TABLE `GachaCollectionItems` ADD COLUMN `SnapshotDescription` varchar(500) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';",
                "SnapshotRewardType" => "ALTER TABLE `GachaCollectionItems` ADD COLUMN `SnapshotRewardType` varchar(40) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';",
                "SnapshotRarity" => "ALTER TABLE `GachaCollectionItems` ADD COLUMN `SnapshotRarity` varchar(40) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';",
                "SnapshotImagePath" => "ALTER TABLE `GachaCollectionItems` ADD COLUMN `SnapshotImagePath` varchar(500) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';",
                "SnapshotLetterTitle" => "ALTER TABLE `GachaCollectionItems` ADD COLUMN `SnapshotLetterTitle` varchar(160) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';",
                "SnapshotLetterContent" => "ALTER TABLE `GachaCollectionItems` ADD COLUMN `SnapshotLetterContent` longtext CHARACTER SET utf8mb4 NULL;",
                _ => throw new InvalidOperationException("Unsupported Gacha collection snapshot column.")
            };

            await _context.Database.ExecuteSqlRawAsync(sql);
        }
    }

    private async Task BackfillCollectionSnapshotsAsync()
    {
        await _context.Database.ExecuteSqlRawAsync("""
            UPDATE `GachaCollectionItems` AS item
            INNER JOIN `GachaRewards` AS reward ON reward.`Id` = item.`GachaRewardId`
            LEFT JOIN `UploadedMedia` AS media ON media.`Id` = reward.`MediaId`
            SET
                item.`SnapshotName` = CASE WHEN item.`SnapshotName` = '' THEN reward.`Name` ELSE item.`SnapshotName` END,
                item.`SnapshotDescription` = CASE WHEN item.`SnapshotDescription` = '' THEN reward.`Description` ELSE item.`SnapshotDescription` END,
                item.`SnapshotRewardType` = CASE
                    WHEN item.`SnapshotRewardType` <> '' THEN item.`SnapshotRewardType`
                    WHEN reward.`RewardType` = 1 THEN 'Doll'
                    WHEN reward.`RewardType` = 2 THEN 'Letter'
                    WHEN reward.`RewardType` = 3 THEN 'Poster'
                    ELSE CAST(reward.`RewardType` AS CHAR)
                END,
                item.`SnapshotRarity` = CASE
                    WHEN item.`SnapshotRarity` <> '' THEN item.`SnapshotRarity`
                    WHEN reward.`Rarity` = 1 THEN 'Common'
                    WHEN reward.`Rarity` = 2 THEN 'Rare'
                    WHEN reward.`Rarity` = 3 THEN 'Epic'
                    WHEN reward.`Rarity` = 4 THEN 'Legendary'
                    ELSE CAST(reward.`Rarity` AS CHAR)
                END,
                item.`SnapshotImagePath` = CASE WHEN item.`SnapshotImagePath` = '' THEN COALESCE(media.`FilePath`, '') ELSE item.`SnapshotImagePath` END,
                item.`SnapshotLetterTitle` = CASE WHEN item.`SnapshotLetterTitle` = '' THEN reward.`LetterTitle` ELSE item.`SnapshotLetterTitle` END,
                item.`SnapshotLetterContent` = CASE WHEN item.`SnapshotLetterContent` = '' OR item.`SnapshotLetterContent` IS NULL THEN reward.`LetterContent` ELSE item.`SnapshotLetterContent` END
            WHERE item.`SnapshotName` = ''
                OR item.`SnapshotRewardType` = ''
                OR item.`SnapshotRarity` = '';
            """);
    }

    private async Task<List<GachaPullDto>> LoadPullsForBatchAsync(int batchId, bool pendingOnly = false)
    {
        var pullsQuery = _context.GachaPulls
            .Include(pull => pull.Reward)
            .ThenInclude(reward => reward!.Media)
            .Where(pull => pull.GachaPullBatchId == batchId)
            .OrderBy(pull => pull.Id);
        if (pendingOnly)
        {
            pullsQuery = pullsQuery.Where(pull => pull.Status == GachaPullStatus.Pending).OrderBy(pull => pull.Id);
        }

        var pulls = await pullsQuery.ToListAsync();

        return pulls
            .Where(pull => pull.Reward is not null)
            .Select(pull => ToPullDto(pull, pull.Reward!))
            .ToList();
    }

    private async Task<int> GetPityCountAsync(int playerProfileId)
    {
        return (await GetOrCreatePityStateAsync(playerProfileId)).LegendaryPityCount;
    }

    private async Task<int> GetBalanceAsync(int playerProfileId)
    {
        return await _context.PlayerProfiles
            .AsNoTracking()
            .Where(player => player.Id == playerProfileId)
            .Select(player => player.TokenBalance)
            .FirstOrDefaultAsync();
    }

    private async Task<GachaPityState> GetOrCreatePityStateAsync(int playerProfileId)
    {
        var pity = await _context.GachaPityStates.FirstOrDefaultAsync(state => state.PlayerProfileId == playerProfileId);
        if (pity is not null)
        {
            return pity;
        }

        pity = new GachaPityState { PlayerProfileId = playerProfileId };
        _context.GachaPityStates.Add(pity);
        await _context.SaveChangesAsync();
        return pity;
    }

    private GeneratedRewards GenerateRewards(IReadOnlyList<GachaReward> rewards, int pullCount, int startingPity)
    {
        var results = new List<GeneratedReward>();
        var pityCount = startingPity;
        var availableRewards = rewards.ToList();

        for (var index = 0; index < pullCount; index++)
        {
            var forceLegendary = pityCount >= LegendaryPityThreshold - 1;
            var pool = forceLegendary
                ? availableRewards.Where(reward => reward.Rarity == GachaRarity.Legendary).ToList()
                : availableRewards;
            var reward = PickWeightedReward(pool);
            results.Add(new GeneratedReward(reward, forceLegendary, false));
            if (!string.IsNullOrWhiteSpace(reward.BuiltInCosmeticId))
            {
                availableRewards.Remove(reward);
            }
            pityCount = reward.Rarity == GachaRarity.Legendary ? 0 : pityCount + 1;
        }

        if (pullCount == 10 && results.All(item => item.Reward.Rarity < GachaRarity.Epic))
        {
            var selectedCosmeticIds = results
                .Where(item => !string.IsNullOrWhiteSpace(item.Reward.BuiltInCosmeticId))
                .Select(item => item.Reward.Id)
                .ToHashSet();
            var reward = PickWeightedReward(rewards
                .Where(item => item.Rarity >= GachaRarity.Epic
                    && (string.IsNullOrWhiteSpace(item.BuiltInCosmeticId) || !selectedCosmeticIds.Contains(item.Id)))
                .ToList());
            results[^1] = new GeneratedReward(reward, false, true);
            pityCount = reward.Rarity == GachaRarity.Legendary ? 0 : pityCount;
        }

        return new GeneratedRewards(results, pityCount);
    }

    private GachaReward PickWeightedReward(IReadOnlyList<GachaReward> rewards)
    {
        var availableRarities = rewards
            .Select(reward => reward.Rarity)
            .Distinct()
            .Select(rarity => new { Rarity = rarity, Weight = RarityWeights.GetValueOrDefault(rarity, 0) })
            .Where(item => item.Weight > 0)
            .ToList();
        var totalWeight = availableRarities.Sum(item => item.Weight);
        var roll = _random.Next(1, totalWeight + 1);
        var runningTotal = 0;

        foreach (var item in availableRarities)
        {
            runningTotal += item.Weight;
            if (roll <= runningTotal)
            {
                var matchingRewards = rewards.Where(reward => reward.Rarity == item.Rarity).ToList();
                return matchingRewards[_random.Next(matchingRewards.Count)];
            }
        }

        return rewards[^1];
    }

    private static int GetRefundValue(GachaRarity rarity)
    {
        return RefundValues.GetValueOrDefault(rarity, 0);
    }

    private static GachaPullDto ToPullDto(GachaPull pull, GachaReward reward)
    {
        return new GachaPullDto(pull.PullToken, ToDto(reward, 0), pull.RefundAmount, pull.IsPityGuaranteed, pull.IsTenPullGuaranteed);
    }

    public static GachaRewardDto ToDto(GachaReward reward, int quantity)
    {
        return new GachaRewardDto(reward.Id, reward.Name, reward.Description, reward.RewardType.ToString(), reward.Rarity.ToString(), reward.Media?.FilePath, reward.LetterTitle, reward.LetterContent, quantity);
    }

    private static GachaRewardDto ToCollectionDto(GachaCollectionItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.SnapshotName))
        {
            return new GachaRewardDto(
                item.GachaRewardId,
                item.SnapshotName,
                item.SnapshotDescription ?? "",
                item.SnapshotRewardType ?? "",
                item.SnapshotRarity ?? "",
                string.IsNullOrWhiteSpace(item.SnapshotImagePath) ? null : item.SnapshotImagePath,
                item.SnapshotLetterTitle ?? "",
                item.SnapshotLetterContent ?? "",
                item.Quantity);
        }

        return ToDto(item.Reward!, item.Quantity);
    }

    private static void ApplySnapshot(GachaCollectionItem item, GachaReward reward)
    {
        item.SnapshotName = reward.Name;
        item.SnapshotDescription = reward.Description;
        item.SnapshotRewardType = reward.RewardType.ToString();
        item.SnapshotRarity = reward.Rarity.ToString();
        item.SnapshotImagePath = reward.Media?.FilePath ?? "";
        item.SnapshotLetterTitle = reward.LetterTitle;
        item.SnapshotLetterContent = reward.LetterContent;
    }

    private sealed record GeneratedRewards(List<GeneratedReward> Rewards, int PityAfter);
    private sealed record GeneratedReward(GachaReward Reward, bool IsPityGuaranteed, bool IsTenPullGuaranteed);
}

public record GachaEconomyStateDto(int Balance, int LegendaryPityCount, int LegendaryPityThreshold, int SinglePullCost, int TenPullCost, IReadOnlyDictionary<string, int> RefundValues);
public record GachaRewardDto(int Id, string Name, string Description, string RewardType, string Rarity, string? ImagePath, string LetterTitle, string LetterContent, int Quantity);
public record GachaPullDto(string PullToken, GachaRewardDto Reward, int RefundAmount, bool IsPityGuaranteed, bool IsTenPullGuaranteed);
public record GachaPullResult(bool Succeeded, bool HasReward, string? BatchToken, string? PullToken, GachaRewardDto? Reward, IReadOnlyList<GachaPullDto> Pulls, int Balance, int PityCount, int RequiredTokens, int MissingTokens, string? Message)
{
    public static GachaPullResult Invalid(string message) => new(false, false, null, null, null, [], 0, 0, 0, 0, message);
    public static GachaPullResult InsufficientTokens(int balance, int required) => new(false, false, null, null, null, [], balance, 0, required, Math.Max(required - balance, 0), $"Not enough tokens. You need {Math.Max(required - balance, 0)} more.");
    public static GachaPullResult Success(string batchToken, IReadOnlyList<GachaPullDto> pulls, int balance, int pityCount) => new(true, pulls.Count > 0, batchToken, pulls.FirstOrDefault()?.PullToken, pulls.FirstOrDefault()?.Reward, pulls, balance, pityCount, 0, 0, null);
}
public record GachaDecisionResult(bool Succeeded, string? Message, GachaRewardDto? Reward, GachaPullStatus? Status, int RefundAmount, int Balance)
{
    public static GachaDecisionResult Success(GachaRewardDto reward, GachaPullStatus status, int refundAmount, int balance) => new(true, null, reward, status, refundAmount, balance);
    public static GachaDecisionResult Invalid(string message) => new(false, message, null, null, 0, 0);
}
