using CasualDate.Data;
using CasualDate.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Services;

public class PlayerProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly DatabaseSchemaService _schemaService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public PlayerProfileService(
        ApplicationDbContext context,
        DatabaseSchemaService schemaService,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _schemaService = schemaService;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<PlayerProfile> GetOrCreateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HTTP context is available.");
        var userId = _userManager.GetUserId(httpContext.User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("A player account is required.");
        }

        if (!await _schemaService.TableExistsAsync("PlayerProfiles"))
        {
            throw new PlayerDataUnavailableException("Player profile tables are not available yet. Run database migrations.");
        }

        var applicationUserId = userId;
        var existingPlayer = await _context.PlayerProfiles
            .FirstOrDefaultAsync(player => player.ApplicationUserId == applicationUserId);

        if (existingPlayer is not null)
        {
            return existingPlayer;
        }

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("The signed-in account could not be found.");
        var player = new PlayerProfile
        {
            ApplicationUserId = userId,
            DisplayName = user.Email ?? user.UserName ?? "Player",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PlayerProfiles.Add(player);
        await _context.SaveChangesAsync();

        return player;
    }
}
