using System.Text.Json;
using CasualDate.Data;
using CasualDate.Models;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Services;

public class BlackjackService
{
    private const int DealerStandValue = 17;
    private const int MinBet = 1;
    private const int MaxBet = 250;

    private readonly ApplicationDbContext _context;
    private readonly DatabaseSchemaService _schemaService;
    private readonly PlayerProfileService _playerProfileService;
    private readonly TokenWalletService _walletService;
    private readonly Random _random = Random.Shared;

    public BlackjackService(
        ApplicationDbContext context,
        DatabaseSchemaService schemaService,
        PlayerProfileService playerProfileService,
        TokenWalletService walletService)
    {
        _context = context;
        _schemaService = schemaService;
        _playerProfileService = playerProfileService;
        _walletService = walletService;
    }

    public async Task<BlackjackStateDto> GetStateAsync()
    {
        await EnsureTableAsync();
        var player = await _playerProfileService.GetOrCreateAsync();
        var activeRound = await GetActiveRoundAsync(player.Id);

        return activeRound is null
            ? BlackjackStateDto.Empty(player.TokenBalance, MinBet, MaxBet)
            : ToDto(activeRound, player.TokenBalance);
    }

    public async Task<BlackjackStateDto> StartRoundAsync(int betAmount)
    {
        await EnsureTableAsync();
        betAmount = Math.Clamp(betAmount, MinBet, MaxBet);

        var player = await _playerProfileService.GetOrCreateAsync();

        var activeRound = await GetActiveRoundAsync(player.Id);
        if (activeRound is not null)
        {
            return ToDto(activeRound, player.TokenBalance);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        if (betAmount > player.TokenBalance)
        {
            throw new InvalidOperationException("You do not have enough tokens for that bet.");
        }

        var deck = BuildShuffledDeck();
        var playerHand = new List<BlackjackCard> { Draw(deck), Draw(deck) };
        var dealerHand = new List<BlackjackCard> { Draw(deck), Draw(deck) };
        var round = new BlackjackRound
        {
            PlayerProfileId = player.Id,
            RoundToken = Guid.NewGuid().ToString("N"),
            BetAmount = betAmount,
            DeckJson = Serialize(deck),
            PlayerHandJson = Serialize(playerHand),
            DealerHandJson = Serialize(dealerHand)
        };

        await _walletService.AdjustBalanceAsync(
            player,
            -betAmount,
            "BlackjackBet",
            round.RoundToken,
            "Blackjack bet placed.");

        if (IsBlackjack(playerHand))
        {
            ResolveRound(round, BlackjackRoundStatus.PlayerBlackjack, betAmount * 3);
            await _walletService.AdjustBalanceAsync(
                player,
                round.PayoutAmount,
                "BlackjackPayout",
                round.RoundToken,
                "Blackjack natural payout.");
        }

        _context.BlackjackRounds.Add(round);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return ToDto(round, player.TokenBalance);
    }

    public async Task<BlackjackStateDto> HitAsync(string roundToken)
    {
        await EnsureTableAsync();
        var player = await _playerProfileService.GetOrCreateAsync();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var round = await GetOwnedRoundAsync(player.Id, roundToken);

        if (round.Status != BlackjackRoundStatus.Active)
        {
            await transaction.CommitAsync();
            return ToDto(round, player.TokenBalance);
        }

        var deck = Deserialize(round.DeckJson);
        var playerHand = Deserialize(round.PlayerHandJson);
        playerHand.Add(Draw(deck));
        round.DeckJson = Serialize(deck);
        round.PlayerHandJson = Serialize(playerHand);
        round.UpdatedAt = DateTime.UtcNow;

        if (HandValue(playerHand) > 21)
        {
            ResolveRound(round, BlackjackRoundStatus.PlayerBust, 0);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return ToDto(round, player.TokenBalance);
    }

    public async Task<BlackjackStateDto> StandAsync(string roundToken)
    {
        await EnsureTableAsync();
        var player = await _playerProfileService.GetOrCreateAsync();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var round = await GetOwnedRoundAsync(player.Id, roundToken);

        if (round.Status != BlackjackRoundStatus.Active)
        {
            await transaction.CommitAsync();
            return ToDto(round, player.TokenBalance);
        }

        var deck = Deserialize(round.DeckJson);
        var playerHand = Deserialize(round.PlayerHandJson);
        var dealerHand = Deserialize(round.DealerHandJson);

        while (HandValue(dealerHand) < DealerStandValue)
        {
            dealerHand.Add(Draw(deck));
        }

        round.DeckJson = Serialize(deck);
        round.DealerHandJson = Serialize(dealerHand);

        var playerValue = HandValue(playerHand);
        var dealerValue = HandValue(dealerHand);
        var status = dealerValue > 21
            ? BlackjackRoundStatus.DealerBust
            : playerValue > dealerValue
                ? BlackjackRoundStatus.PlayerWin
                : playerValue == dealerValue
                    ? BlackjackRoundStatus.Push
                    : BlackjackRoundStatus.DealerWin;
        var payout = status switch
        {
            BlackjackRoundStatus.DealerBust or BlackjackRoundStatus.PlayerWin => round.BetAmount * 2,
            BlackjackRoundStatus.Push => round.BetAmount,
            _ => 0
        };

        ResolveRound(round, status, payout);

        if (payout > 0)
        {
            await _walletService.AdjustBalanceAsync(
                player,
                payout,
                "BlackjackPayout",
                round.RoundToken,
                "Blackjack round payout.");
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return ToDto(round, player.TokenBalance);
    }

    private async Task EnsureTableAsync()
    {
        if (!await _schemaService.TableExistsAsync("BlackjackRounds"))
        {
            throw new PlayerDataUnavailableException("Blackjack tables are not available yet. Restart the app or run database migrations.");
        }
    }

    private async Task<BlackjackRound?> GetActiveRoundAsync(int playerProfileId)
    {
        return await _context.BlackjackRounds
            .Where(round => round.PlayerProfileId == playerProfileId && round.Status == BlackjackRoundStatus.Active)
            .OrderByDescending(round => round.CreatedAt)
            .FirstOrDefaultAsync();
    }

    private async Task<BlackjackRound> GetOwnedRoundAsync(int playerProfileId, string roundToken)
    {
        var round = await _context.BlackjackRounds
            .FirstOrDefaultAsync(item => item.PlayerProfileId == playerProfileId && item.RoundToken == roundToken);

        return round ?? throw new InvalidOperationException("That Blackjack round could not be found.");
    }

    private void ResolveRound(BlackjackRound round, string status, int payout)
    {
        round.Status = status;
        round.PayoutAmount = payout;
        round.ResolvedAt = DateTime.UtcNow;
        round.UpdatedAt = DateTime.UtcNow;
    }

    private List<BlackjackCard> BuildShuffledDeck()
    {
        var suits = new[] { "hearts", "diamonds", "clubs", "spades" };
        var ranks = new[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        var deck = suits
            .SelectMany(suit => ranks.Select(rank => new BlackjackCard(rank, suit)))
            .OrderBy(_ => _random.Next())
            .ToList();

        return deck;
    }

    private static BlackjackCard Draw(List<BlackjackCard> deck)
    {
        var card = deck[^1];
        deck.RemoveAt(deck.Count - 1);
        return card;
    }

    private static int HandValue(IReadOnlyList<BlackjackCard> cards)
    {
        var total = 0;
        var aces = 0;

        foreach (var card in cards)
        {
            if (card.Rank == "A")
            {
                total += 11;
                aces += 1;
            }
            else if (card.Rank is "J" or "Q" or "K")
            {
                total += 10;
            }
            else
            {
                total += int.Parse(card.Rank);
            }
        }

        while (total > 21 && aces > 0)
        {
            total -= 10;
            aces -= 1;
        }

        return total;
    }

    private static bool IsBlackjack(IReadOnlyList<BlackjackCard> cards)
    {
        return cards.Count == 2 && HandValue(cards) == 21;
    }

    private static string Serialize(List<BlackjackCard> cards)
    {
        return JsonSerializer.Serialize(cards);
    }

    private static List<BlackjackCard> Deserialize(string json)
    {
        return JsonSerializer.Deserialize<List<BlackjackCard>>(json) ?? [];
    }

    private static BlackjackStateDto ToDto(BlackjackRound round, int balance)
    {
        var playerHand = Deserialize(round.PlayerHandJson);
        var dealerHand = Deserialize(round.DealerHandJson);
        var isActive = round.Status == BlackjackRoundStatus.Active;

        return new BlackjackStateDto(
            true,
            round.RoundToken,
            balance,
            MinBet,
            MaxBet,
            round.BetAmount,
            round.Status,
            round.PayoutAmount,
            playerHand,
            isActive ? dealerHand.Take(1).ToList() : dealerHand,
            HandValue(playerHand),
            isActive ? null : HandValue(dealerHand),
            GetMessage(round.Status, round.BetAmount, round.PayoutAmount),
            isActive);
    }

    private static string GetMessage(string status, int bet, int payout)
    {
        return status switch
        {
            BlackjackRoundStatus.Active => "Hit for another card or stand to let the dealer play.",
            BlackjackRoundStatus.PlayerBlackjack => $"Blackjack. Natural win pays {payout} tokens.",
            BlackjackRoundStatus.PlayerWin => $"You won {payout} tokens.",
            BlackjackRoundStatus.DealerBust => $"Dealer busted. You won {payout} tokens.",
            BlackjackRoundStatus.Push => $"Push. Your {bet} token bet was returned.",
            BlackjackRoundStatus.PlayerBust => "Bust. The dealer takes this round.",
            BlackjackRoundStatus.DealerWin => "Dealer wins this round.",
            _ => "Round finished."
        };
    }
}

public sealed record BlackjackCard(string Rank, string Suit);

public sealed record BlackjackStateDto(
    bool HasRound,
    string? RoundToken,
    int Balance,
    int MinBet,
    int MaxBet,
    int BetAmount,
    string Status,
    int PayoutAmount,
    IReadOnlyList<BlackjackCard> PlayerHand,
    IReadOnlyList<BlackjackCard> DealerHand,
    int PlayerValue,
    int? DealerValue,
    string Message,
    bool CanAct)
{
    public static BlackjackStateDto Empty(int balance, int minBet, int maxBet) => new(
        false,
        null,
        balance,
        minBet,
        maxBet,
        0,
        "Ready",
        0,
        [],
        [],
        0,
        null,
        "Place a token bet to deal.",
        false);
}
