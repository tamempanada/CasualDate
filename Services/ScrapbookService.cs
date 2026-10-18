using CasualDate.Data;
using CasualDate.Models;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Services;

public class ScrapbookService
{
    private const int DefaultPageCount = 8;
    private static readonly string[] PageDesigns = ["lined", "blank", "dotted", "floral"];

    private readonly ApplicationDbContext _context;
    private readonly DatabaseSchemaService _schemaService;
    private readonly PlayerProfileService _playerProfileService;

    public ScrapbookService(
        ApplicationDbContext context,
        DatabaseSchemaService schemaService,
        PlayerProfileService playerProfileService)
    {
        _context = context;
        _schemaService = schemaService;
        _playerProfileService = playerProfileService;
    }

    public async Task<IReadOnlyList<ScrapbookPageDto>> GetPagesAsync()
    {
        await EnsureSchemaAsync();
        var player = await _playerProfileService.GetOrCreateAsync();
        await EnsureDefaultPagesAsync(player.Id);

        return await _context.ScrapbookPages
            .Where(page => page.PlayerProfileId == player.Id)
            .OrderBy(page => page.PageNumber)
            .Select(page => ToDto(page))
            .ToListAsync();
    }

    public async Task<ScrapbookPageDto> SavePageAsync(ScrapbookPageSaveRequest request)
    {
        await EnsureSchemaAsync();
        var player = await _playerProfileService.GetOrCreateAsync();
        var pageNumber = Math.Clamp(request.PageNumber, 1, DefaultPageCount);
        var page = await _context.ScrapbookPages
            .FirstOrDefaultAsync(item => item.PlayerProfileId == player.Id && item.PageNumber == pageNumber);

        if (page is null)
        {
            page = CreateDefaultPage(player.Id, pageNumber);
            _context.ScrapbookPages.Add(page);
        }

        page.Title = NormalizeText(request.Title, 120);
        page.DateLabel = NormalizeText(request.DateLabel, 80);
        page.Body = request.Body?.Trim() ?? "";
        page.PageDesign = NormalizeDesign(request.PageDesign, pageNumber);
        page.TextColor = NormalizeColor(request.TextColor);
        page.TextAlign = NormalizeAlign(request.TextAlign);
        page.FontSize = Math.Clamp(request.FontSize, 14, 28);
        page.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToDto(page);
    }

    private async Task EnsureSchemaAsync()
    {
        if (!await _schemaService.TableExistsAsync("ScrapbookPages"))
        {
            throw new PlayerDataUnavailableException("Scrapbook tables are not available yet. Restart the app or run database migrations.");
        }
    }

    private async Task EnsureDefaultPagesAsync(int playerProfileId)
    {
        var existingPageNumbers = await _context.ScrapbookPages
            .Where(page => page.PlayerProfileId == playerProfileId)
            .Select(page => page.PageNumber)
            .ToListAsync();

        var now = DateTime.UtcNow;

        for (var pageNumber = 1; pageNumber <= DefaultPageCount; pageNumber++)
        {
            if (existingPageNumbers.Contains(pageNumber))
            {
                continue;
            }

            var page = CreateDefaultPage(playerProfileId, pageNumber);
            page.CreatedAt = now;
            page.UpdatedAt = now;
            _context.ScrapbookPages.Add(page);
        }

        await _context.SaveChangesAsync();
    }

    private static ScrapbookPage CreateDefaultPage(int playerProfileId, int pageNumber)
    {
        return new ScrapbookPage
        {
            PlayerProfileId = playerProfileId,
            PageNumber = pageNumber,
            Title = pageNumber == 1 ? "First page" : $"Page {pageNumber}",
            DateLabel = DateTime.Today.ToString("MMMM d, yyyy"),
            PageDesign = PageDesigns[(pageNumber - 1) % PageDesigns.Length],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static string NormalizeDesign(string? value, int pageNumber)
    {
        var design = NormalizeText(value, 40).ToLowerInvariant();
        return PageDesigns.Contains(design) ? design : PageDesigns[(pageNumber - 1) % PageDesigns.Length];
    }

    private static string NormalizeAlign(string? value)
    {
        var align = NormalizeText(value, 20).ToLowerInvariant();
        return align is "left" or "center" or "right" ? align : "left";
    }

    private static string NormalizeColor(string? value)
    {
        var color = NormalizeText(value, 20);
        return color.StartsWith('#') && color.Length is 4 or 7 ? color : "#3a2724";
    }

    private static string NormalizeText(string? value, int maxLength)
    {
        var text = value?.Trim() ?? "";
        return text.Length <= maxLength ? text : text[..maxLength];
    }

    private static ScrapbookPageDto ToDto(ScrapbookPage page)
    {
        return new ScrapbookPageDto(
            page.PageNumber,
            page.Title,
            page.DateLabel,
            page.Body,
            page.PageDesign,
            page.TextColor,
            page.TextAlign,
            page.FontSize,
            page.UpdatedAt);
    }
}

public sealed record ScrapbookPageDto(
    int PageNumber,
    string Title,
    string DateLabel,
    string Body,
    string PageDesign,
    string TextColor,
    string TextAlign,
    int FontSize,
    DateTime UpdatedAt);

public sealed class ScrapbookPageSaveRequest
{
    public int PageNumber { get; set; }

    public string Title { get; set; } = "";

    public string DateLabel { get; set; } = "";

    public string Body { get; set; } = "";

    public string PageDesign { get; set; } = "lined";

    public string TextColor { get; set; } = "#3a2724";

    public string TextAlign { get; set; } = "left";

    public int FontSize { get; set; } = 18;
}
