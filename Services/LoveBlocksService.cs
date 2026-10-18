using System.Data.Common;
using System.Text.Json;
using CasualDate.Data;
using CasualDate.Models;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Services;

public class LoveBlocksService
{
    private const int BoardSize = 8;
    private const int PointsPerPlacedCell = 1;
    private const int PointsPerClearedLine = 10;
    private const int MultiLineBonusPerExtraLine = 5;
    private const string ScoreRewardKey = "love-blocks:score";
    private const string LineRewardKey = "love-blocks:lines";
    private const string BestRewardKey = "love-blocks:personal-best";
    private static readonly int[][][] Shapes =
    [
        [[0,0]], [[0,0],[0,1]], [[0,0],[1,0]], [[0,0],[0,1],[1,0],[1,1]],
        [[0,0],[0,1],[0,2]], [[0,0],[1,0],[2,0]], [[0,0],[0,1],[0,2],[1,0]],
        [[0,0],[0,1],[1,1],[1,2]], [[0,1],[1,0],[1,1],[1,2]],
        [[0,0],[1,0],[1,1],[2,1]], [[0,0],[0,1],[0,2],[1,2],[2,2]],
        [[0,0],[1,0],[2,0],[2,1],[2,2]], [[0,0],[0,1],[0,2],[0,3]],
        [[0,0],[1,0],[2,0],[3,0]], [[0,0],[0,1],[0,2],[0,3],[1,0]],
        [[0,1],[0,2],[1,1],[1,2],[2,1],[2,2]], [[0,0],[0,1],[1,0],[1,1],[2,0],[2,1]],
        [[0,0],[1,0],[2,0],[3,0],[4,0]], [[0,0],[0,1],[0,2],[0,3],[0,4]]
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ApplicationDbContext _db;
    private readonly DatabaseSchemaService _schema;
    private readonly PlayerProfileService _players;

    public LoveBlocksService(ApplicationDbContext db, DatabaseSchemaService schema, PlayerProfileService players)
    {
        _db = db;
        _schema = schema;
        _players = players;
    }

    public async Task<LoveBlocksState> StartAsync(bool restart = false)
    {
        await EnsureSchemaAsync();
        var player = await _players.GetOrCreateAsync();
        await using var tx = await _db.Database.BeginTransactionAsync();
        await LockPlayerAsync(player.Id);
        var active = await _db.LoveBlocksRuns
            .Where(run => run.PlayerProfileId == player.Id && !run.IsOver)
            .OrderByDescending(run => run.Id)
            .FirstOrDefaultAsync();
        if (active is not null && !restart)
        {
            await tx.CommitAsync();
            return await StateAsync(active, player.Id);
        }
        if (active is null && !restart)
        {
            var previous = await _db.LoveBlocksRuns
                .Where(run => run.PlayerProfileId == player.Id)
                .OrderByDescending(run => run.Id)
                .FirstOrDefaultAsync();
            if (previous is not null)
            {
                await tx.CommitAsync();
                return await StateAsync(previous, player.Id);
            }
        }
        if (active is not null)
        {
            active.IsOver = true;
            active.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        var settings = await _db.TokenRewardSettings
            .Where(setting => setting.EventKey == ScoreRewardKey || setting.EventKey == LineRewardKey || setting.EventKey == BestRewardKey)
            .ToDictionaryAsync(setting => setting.EventKey);
        var score = settings.GetValueOrDefault(ScoreRewardKey);
        var lines = settings.GetValueOrDefault(LineRewardKey);
        var best = settings.GetValueOrDefault(BestRewardKey);
        var run = new LoveBlocksRun
        {
            PlayerProfileId = player.Id,
            RunToken = Guid.NewGuid().ToString("N"),
            PiecesJson = Serialize(NewPieces()),
            ScoreRewardAmount = score?.TokenAmount ?? 5,
            ScoreRewardInterval = score?.TriggerInterval ?? 500,
            ScoreRewardEnabled = score?.IsEnabled ?? true,
            LineRewardAmount = lines?.TokenAmount ?? 5,
            LineRewardInterval = lines?.TriggerInterval ?? 5,
            LineRewardEnabled = lines?.IsEnabled ?? true,
            BestRewardAmount = best?.TokenAmount ?? 20,
            BestRewardEnabled = best?.IsEnabled ?? true
        };
        _db.LoveBlocksRuns.Add(run);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return await StateAsync(run, player.Id);
    }

    public async Task<LoveBlocksState> PlaceAsync(string runToken, int pieceIndex, int row, int column, string requestId)
    {
        await EnsureSchemaAsync();
        if (string.IsNullOrWhiteSpace(runToken) || !Guid.TryParse(requestId, out _))
            throw new InvalidOperationException("This move is invalid. Restart the game and try again.");

        var player = await _players.GetOrCreateAsync();
        await using var tx = await _db.Database.BeginTransactionAsync();
        player.TokenBalance = await LockPlayerAsync(player.Id);
        var run = await _db.LoveBlocksRuns
            .FromSqlInterpolated($"SELECT * FROM `LoveBlocksRuns` WHERE `RunToken` = {runToken} AND `PlayerProfileId` = {player.Id} FOR UPDATE")
            .FirstOrDefaultAsync();
        if (run is null) throw new InvalidOperationException("This run could not be found. Start a new run to keep playing.");
        var processedRequestIds = JsonSerializer.Deserialize<HashSet<string>>(run.ProcessedRequestIdsJson, JsonOptions) ?? [];
        if (processedRequestIds.Contains(requestId))
        {
            await tx.CommitAsync();
            return await StateAsync(run, player.Id);
        }
        if (run.IsOver) throw new InvalidOperationException("This run has ended. Start a new run to keep playing.");

        var board = run.Board.ToCharArray();
        var pieces = Deserialize(run.PiecesJson);
        if (pieceIndex < 0 || pieceIndex >= pieces.Count || row < 0 || column < 0)
            throw new InvalidOperationException("That piece placement is not valid.");

        var piece = pieces[pieceIndex];
        var indexes = piece.Cells.Select(cell => (R: row + cell.R, C: column + cell.C)).ToList();
        if (indexes.Any(cell => cell.R >= BoardSize || cell.C >= BoardSize || board[cell.R * BoardSize + cell.C] != '.'))
            throw new InvalidOperationException("That piece does not fit there.");

        foreach (var cell in indexes) board[cell.R * BoardSize + cell.C] = '#';
        var completedRows = Enumerable.Range(0, BoardSize).Where(r => Enumerable.Range(0, BoardSize).All(c => board[r * BoardSize + c] == '#')).ToHashSet();
        var completedColumns = Enumerable.Range(0, BoardSize).Where(c => Enumerable.Range(0, BoardSize).All(r => board[r * BoardSize + c] == '#')).ToHashSet();
        var cleared = new HashSet<int>();
        foreach (var r in completedRows) for (var c = 0; c < BoardSize; c++) cleared.Add(r * BoardSize + c);
        foreach (var c in completedColumns) for (var r = 0; r < BoardSize; r++) cleared.Add(r * BoardSize + c);
        foreach (var index in cleared) board[index] = '.';

        var lines = completedRows.Count + completedColumns.Count;
        var placedPoints = piece.Cells.Count;
        var points = placedPoints * PointsPerPlacedCell + lines * PointsPerClearedLine + (lines > 1 ? (lines - 1) * MultiLineBonusPerExtraLine : 0);
        run.Score += points;
        run.LinesCleared += lines;
        run.MovesMade++;
        run.Board = new string(board);
        pieces.RemoveAt(pieceIndex);
        if (pieces.Count == 0) pieces = NewPieces();
        run.PiecesJson = Serialize(pieces);
        run.LastRequestId = requestId;
        processedRequestIds.Add(requestId);
        run.ProcessedRequestIdsJson = Serialize(processedRequestIds);

        var record = await _db.LoveBlocksPlayerRecords.FirstOrDefaultAsync(item => item.PlayerProfileId == player.Id);
        record ??= new LoveBlocksPlayerRecord { PlayerProfileId = player.Id };
        var isNewBest = run.Score > record.BestScore;
        if (isNewBest)
        {
            record.BestScore = run.Score;
            record.UpdatedAt = DateTime.UtcNow;
            if (record.Id == 0) _db.LoveBlocksPlayerRecords.Add(record);
        }

        var runEarned = 0;
        if (run.ScoreRewardEnabled && run.ScoreRewardInterval > 0)
        {
            var reached = run.Score / run.ScoreRewardInterval;
            for (var milestone = run.ScoreMilestonesAwarded + 1; milestone <= reached; milestone++)
                runEarned += await AwardAsync(player, run, $"score:{milestone}", run.ScoreRewardAmount, $"Love Blocks score milestone {milestone * run.ScoreRewardInterval}");
            run.ScoreMilestonesAwarded = Math.Max(run.ScoreMilestonesAwarded, reached);
        }
        if (run.LineRewardEnabled && run.LineRewardInterval > 0)
        {
            var reached = run.LinesCleared / run.LineRewardInterval;
            for (var milestone = run.LineMilestonesAwarded + 1; milestone <= reached; milestone++)
                runEarned += await AwardAsync(player, run, $"lines:{milestone}", run.LineRewardAmount, $"Love Blocks line milestone {milestone * run.LineRewardInterval} lines");
            run.LineMilestonesAwarded = Math.Max(run.LineMilestonesAwarded, reached);
        }
        if (isNewBest && run.BestRewardEnabled && !run.PersonalBestRewarded)
        {
            runEarned += await AwardAsync(player, run, "personal-best", run.BestRewardAmount, "New Love Blocks personal best");
            run.PersonalBestRewarded = true;
        }

        run.IsOver = !pieces.Any(candidate => CanFit(board, candidate));
        run.UpdatedAt = DateTime.UtcNow;
        run.TokensEarned += runEarned;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return await StateAsync(run, player.Id, points, lines, runEarned, cleared.ToList(), indexes.Select(cell => cell.R * BoardSize + cell.C).ToList());
    }

    private async Task<int> AwardAsync(PlayerProfile player, LoveBlocksRun run, string milestone, int amount, string label)
    {
        if (amount <= 0) return 0;
        var eventKey = $"love-blocks:{run.RunToken}:{milestone}";
        var now = DateTime.UtcNow;
        var rows = await _db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE `PlayerProfiles` SET `TokenBalance` = `TokenBalance` + {amount}, `UpdatedAt` = {now}
            WHERE `Id` = {player.Id};
            """);
        if (rows != 1) throw new InvalidOperationException("Your wallet could not be updated.");
        player.TokenBalance += amount;
        _db.PlayerGameCompletions.Add(new PlayerGameCompletion { PlayerProfileId = player.Id, EventKey = eventKey, TokenAmountAwarded = amount, CompletedAt = now });
        _db.TokenTransactions.Add(new TokenTransaction { PlayerProfileId = player.Id, Amount = amount, BalanceAfter = player.TokenBalance, TransactionType = "LoveBlocksReward", ReferenceKey = eventKey, Description = label, CreatedAt = now });
        return amount;
    }

    private async Task<LoveBlocksState> StateAsync(LoveBlocksRun run, int playerId, int lastMovePoints = 0, int lastMoveLines = 0, int runEarned = 0, List<int>? clearedCells = null, List<int>? placedCells = null)
    {
        var best = await _db.LoveBlocksPlayerRecords.Where(item => item.PlayerProfileId == playerId).Select(item => (int?)item.BestScore).FirstOrDefaultAsync() ?? 0;
        var balance = await _db.PlayerProfiles.Where(item => item.Id == playerId).Select(item => item.TokenBalance).FirstAsync();
        return new LoveBlocksState(run.RunToken, run.Board, Deserialize(run.PiecesJson), run.Score, best, run.LinesCleared, run.MovesMade,
            run.IsOver, lastMovePoints, lastMoveLines, run.TokensEarned, runEarned, balance,
            run.ScoreRewardEnabled ? run.ScoreRewardInterval - run.Score % run.ScoreRewardInterval : 0,
            run.LineRewardEnabled ? run.LineRewardInterval - run.LinesCleared % run.LineRewardInterval : 0,
            clearedCells ?? [], placedCells ?? [], run.ScoreRewardAmount, run.ScoreRewardInterval, run.ScoreRewardEnabled,
            run.LineRewardAmount, run.LineRewardInterval, run.LineRewardEnabled, run.BestRewardAmount, run.BestRewardEnabled);
    }

    private async Task EnsureSchemaAsync()
    {
        if (!await _schema.TableExistsAsync("LoveBlocksRuns") || !await _schema.TableExistsAsync("LoveBlocksPlayerRecords"))
            throw new PlayerDataUnavailableException("Love Blocks is not ready yet. Restart the app or run database migrations.");
    }

    private async Task<int> LockPlayerAsync(int playerId)
    {
        await _db.PlayerProfiles
            .FromSqlInterpolated($"SELECT * FROM `PlayerProfiles` WHERE `Id` = {playerId} FOR UPDATE")
            .Select(profile => profile.Id)
            .FirstAsync();
        var balance = await _db.PlayerProfiles.AsNoTracking()
            .Where(profile => profile.Id == playerId)
            .Select(profile => profile.TokenBalance)
            .FirstAsync();
        var tracked = _db.PlayerProfiles.Local.FirstOrDefault(profile => profile.Id == playerId);
        if (tracked is not null) tracked.TokenBalance = balance;
        return balance;
    }

    private static List<LoveBlocksPiece> NewPieces() => Enumerable.Range(0, 3).Select(_ =>
    {
        var shape = Shapes[Random.Shared.Next(Shapes.Length)];
        return new LoveBlocksPiece(shape.Select(cell => new LoveBlocksCell(cell[0], cell[1])).ToList());
    }).ToList();

    private static bool CanFit(char[] board, LoveBlocksPiece piece)
    {
        for (var r = 0; r < BoardSize; r++) for (var c = 0; c < BoardSize; c++)
            if (piece.Cells.All(cell => r + cell.R < BoardSize && c + cell.C < BoardSize && board[(r + cell.R) * BoardSize + c + cell.C] == '.')) return true;
        return false;
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);
    private static List<LoveBlocksPiece> Deserialize(string value) => JsonSerializer.Deserialize<List<LoveBlocksPiece>>(value, JsonOptions) ?? [];
}

public sealed record LoveBlocksCell(int R, int C);
public sealed record LoveBlocksPiece(List<LoveBlocksCell> Cells);
public sealed record LoveBlocksState(string RunToken, string Board, List<LoveBlocksPiece> Pieces, int Score, int BestScore, int LinesCleared, int MovesMade,
    bool IsOver, int LastMovePoints, int LastMoveLines, int RunEarned, int LastMoveEarned, int TokenBalance, int ScoreProgress, int LineProgress,
    List<int> ClearedCells, List<int> PlacedCells, int ScoreRewardAmount, int ScoreRewardInterval, bool ScoreRewardEnabled, int LineRewardAmount, int LineRewardInterval, bool LineRewardEnabled,
    int BestRewardAmount, bool BestRewardEnabled);
