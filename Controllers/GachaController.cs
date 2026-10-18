using System.Data.Common;
using CasualDate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CasualDate.Controllers;

[Route("gacha")]
public class GachaApiController : Controller
{
    private readonly GachaService _gachaService;

    public GachaApiController(GachaService gachaService)
    {
        _gachaService = gachaService;
    }

    [HttpGet("collection")]
    public async Task<IActionResult> Collection()
    {
        try
        {
            var rewards = await _gachaService.GetCollectionAsync();
            return Json(new { rewards });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Player login required." });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Gacha." });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }

    [HttpGet("state")]
    public async Task<IActionResult> State()
    {
        try
        {
            return Json(new { economy = await _gachaService.GetEconomyStateAsync() });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Player login required." });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Gacha." });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }

    [HttpGet("pending")]
    public async Task<IActionResult> Pending()
    {
        try
        {
            var pendingPulls = await _gachaService.GetLatestPendingPullsAsync();
            return Json(new { pendingPulls });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Player login required." });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Gacha." });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }

    [HttpPost("pull")]
    public async Task<IActionResult> Pull([FromBody] GachaPullRequest request)
    {
        try
        {
            var result = await _gachaService.PullAsync(request.PullCount, request.RequestToken);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Json(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Player login required." });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Gacha." });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }

    [HttpPost("keep")]
    public async Task<IActionResult> Keep([FromBody] GachaDecisionRequest request)
    {
        return await ResolveDecisionAsync(request.PullToken, keep: true);
    }

    [HttpPost("trash")]
    public async Task<IActionResult> Trash([FromBody] GachaDecisionRequest request)
    {
        return await ResolveDecisionAsync(request.PullToken, keep: false);
    }

    private async Task<IActionResult> ResolveDecisionAsync(string pullToken, bool keep)
    {
        try
        {
            var result = keep
                ? await _gachaService.KeepAsync(pullToken)
                : await _gachaService.TrashAsync(pullToken);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Message });
            }

            return Json(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Player login required." });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Gacha." });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }
}

public sealed class GachaDecisionRequest
{
    public string PullToken { get; set; } = "";
}

public sealed class GachaPullRequest
{
    public int PullCount { get; set; } = 1;

    public string RequestToken { get; set; } = "";
}
