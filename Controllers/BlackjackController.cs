using System.Data.Common;
using CasualDate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CasualDate.Controllers;

[Route("blackjack")]
public class BlackjackController : Controller
{
    private readonly BlackjackService _blackjackService;

    public BlackjackController(BlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }

    [HttpGet("state")]
    public async Task<IActionResult> State()
    {
        try
        {
            return Json(new { game = await _blackjackService.GetStateAsync() });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Player login required." });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Blackjack." });
        }
    }

    [HttpPost("deal")]
    public async Task<IActionResult> Deal([FromBody] BlackjackDealRequest request)
    {
        try
        {
            return Json(new { game = await _blackjackService.StartRoundAsync(request.BetAmount) });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Player login required." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Blackjack." });
        }
    }

    [HttpPost("hit")]
    public async Task<IActionResult> Hit([FromBody] BlackjackRoundRequest request)
    {
        return await PlayAsync(() => _blackjackService.HitAsync(request.RoundToken));
    }

    [HttpPost("stand")]
    public async Task<IActionResult> Stand([FromBody] BlackjackRoundRequest request)
    {
        return await PlayAsync(() => _blackjackService.StandAsync(request.RoundToken));
    }

    private async Task<IActionResult> PlayAsync(Func<Task<BlackjackStateDto>> action)
    {
        try
        {
            return Json(new { game = await action() });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Player login required." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Blackjack." });
        }
    }
}

public sealed class BlackjackDealRequest
{
    public int BetAmount { get; set; }
}

public sealed class BlackjackRoundRequest
{
    public string RoundToken { get; set; } = "";
}
