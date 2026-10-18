using CasualDate.Data;
using CasualDate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CasualDate.Services;

public class TokenWalletService
{
    private readonly ApplicationDbContext _context;
    private readonly DatabaseSchemaService _schemaService;
    private readonly PlayerProfileService _playerProfileService;

    public TokenWalletService(
        ApplicationDbContext context,
        DatabaseSchemaService schemaService,
        PlayerProfileService playerProfileService)
    {
        _context = context;
        _schemaService = schemaService;
        _playerProfileService = playerProfileService;
    }

    public async Task<TokenWalletSnapshot> GetSnapshotAsync()
    {
        var player = await _playerProfileService.GetOrCreateAsync();
        return new TokenWalletSnapshot(player.TokenBalance);
    }

    public async Task EnsureRewardSettingAsync(string eventKey, string displayName, int defaultAmount)
    {
        if (!await _schemaService.TableExistsAsync("TokenRewardSettings"))
        {
            throw new PlayerDataUnavailableException("Token wallet tables are not available yet. Run database migrations.");
        }

        if (await _context.TokenRewardSettings.AnyAsync(setting => setting.EventKey == eventKey))
        {
            return;
        }

        var now = DateTime.UtcNow;
        _context.TokenRewardSettings.Add(new TokenRewardSetting
        {
            EventKey = eventKey,
            DisplayName = displayName,
            TokenAmount = Math.Clamp(defaultAmount, 0, 10000),
            IsEnabled = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        await _context.SaveChangesAsync();
    }

    public async Task<TokenAwardResult> AwardCompletionAsync(
        string eventKey,
        string? rewardSettingKey = null,
        IDbContextTransaction? existingTransaction = null)
    {
        var player = await _playerProfileService.GetOrCreateAsync();
        var configuredRewardKey = string.IsNullOrWhiteSpace(rewardSettingKey)
            ? eventKey
            : rewardSettingKey;

        if (!await _schemaService.TableExistsAsync("TokenRewardSettings"))
        {
            throw new PlayerDataUnavailableException("Token wallet tables are not available yet. Run database migrations.");
        }

        var reward = await _context.TokenRewardSettings
            .FirstOrDefaultAsync(setting => setting.EventKey == configuredRewardKey && setting.IsEnabled);

        if (reward is null)
        {
            return new TokenAwardResult(false, 0, player.TokenBalance, "No token reward is configured for this event.");
        }

        var existingCompletion = await _context.PlayerGameCompletions
            .FirstOrDefaultAsync(completion => completion.PlayerProfileId == player.Id && completion.EventKey == eventKey);

        if (existingCompletion is not null)
        {
            return new TokenAwardResult(false, existingCompletion.TokenAmountAwarded, player.TokenBalance, "Reward already claimed.");
        }

        var ownsTransaction = existingTransaction is null;
        await using var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync()
            : null;
        var now = DateTime.UtcNow;
        player.TokenBalance += reward.TokenAmount;
        player.UpdatedAt = now;

        _context.PlayerGameCompletions.Add(new PlayerGameCompletion
        {
            PlayerProfileId = player.Id,
            EventKey = eventKey,
            TokenAmountAwarded = reward.TokenAmount,
            CompletedAt = now
        });

        _context.TokenTransactions.Add(new TokenTransaction
        {
            PlayerProfileId = player.Id,
            Amount = reward.TokenAmount,
            BalanceAfter = player.TokenBalance,
            TransactionType = "CompletionReward",
            ReferenceKey = eventKey,
            Description = reward.DisplayName,
            CreatedAt = now
        });

        await _context.SaveChangesAsync();
        if (ownsTransaction)
        {
            await transaction!.CommitAsync();
        }

        return new TokenAwardResult(true, reward.TokenAmount, player.TokenBalance, "Tokens awarded.");
    }

    public async Task<int> AdjustBalanceAsync(
        PlayerProfile player,
        int amount,
        string transactionType,
        string referenceKey,
        string description)
    {
        if (player.TokenBalance + amount < 0)
        {
            throw new InvalidOperationException("Not enough tokens.");
        }

        player.TokenBalance += amount;
        player.UpdatedAt = DateTime.UtcNow;

        _context.TokenTransactions.Add(new TokenTransaction
        {
            PlayerProfileId = player.Id,
            Amount = amount,
            BalanceAfter = player.TokenBalance,
            TransactionType = transactionType,
            ReferenceKey = referenceKey,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });

        return player.TokenBalance;
    }

    public async Task<TokenSpendResult> TrySpendAsync(
        int playerProfileId,
        int amount,
        string transactionType,
        string referenceKey,
        string description)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        var now = DateTime.UtcNow;
        var rowsUpdated = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE `PlayerProfiles`
            SET `TokenBalance` = `TokenBalance` - {amount},
                `UpdatedAt` = {now}
            WHERE `Id` = {playerProfileId}
                AND `TokenBalance` >= {amount};
            """);

        var balance = await _context.PlayerProfiles
            .AsNoTracking()
            .Where(player => player.Id == playerProfileId)
            .Select(player => player.TokenBalance)
            .FirstOrDefaultAsync();

        if (rowsUpdated == 0)
        {
            return new TokenSpendResult(false, balance);
        }

        _context.TokenTransactions.Add(new TokenTransaction
        {
            PlayerProfileId = playerProfileId,
            Amount = -amount,
            BalanceAfter = balance,
            TransactionType = transactionType,
            ReferenceKey = referenceKey,
            Description = description,
            CreatedAt = now
        });

        return new TokenSpendResult(true, balance);
    }

    public async Task<int> AddBalanceAtomicallyAsync(
        int playerProfileId,
        int amount,
        string transactionType,
        string referenceKey,
        string description)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        var now = DateTime.UtcNow;
        await _context.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE `PlayerProfiles`
            SET `TokenBalance` = `TokenBalance` + {amount},
                `UpdatedAt` = {now}
            WHERE `Id` = {playerProfileId};
            """);

        var balance = await _context.PlayerProfiles
            .AsNoTracking()
            .Where(player => player.Id == playerProfileId)
            .Select(player => player.TokenBalance)
            .FirstOrDefaultAsync();

        _context.TokenTransactions.Add(new TokenTransaction
        {
            PlayerProfileId = playerProfileId,
            Amount = amount,
            BalanceAfter = balance,
            TransactionType = transactionType,
            ReferenceKey = referenceKey,
            Description = description,
            CreatedAt = now
        });

        return balance;
    }
}

public sealed record TokenWalletSnapshot(int Balance);

public sealed record TokenAwardResult(bool Awarded, int Amount, int Balance, string Message);
public sealed record TokenSpendResult(bool Succeeded, int Balance);
