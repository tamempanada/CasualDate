using System.Data.Common;
using CasualDate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CasualDate.Controllers;

[Route("hangman")]
public class HangmanController : Controller
{
    private readonly HangmanService _hangmanService;

    public HangmanController(HangmanService hangmanService)
    {
        _hangmanService = hangmanService;
    }

    [HttpGet("progress")]
    public async Task<IActionResult> Progress()
    {
        try
        {
            return Json(new { progress = await _hangmanService.GetProgressAsync() });
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
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Hangman." });
        }
    }

    [HttpPost("adventure-complete")]
    public async Task<IActionResult> AdventureComplete([FromBody] HangmanLevelRequest request)
    {
        try
        {
            var result = await _hangmanService.CompleteAdventureLevelAsync(request.Level);
            return Json(new { progress = result.Progress, reward = result.Reward });
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
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Hangman." });
        }
    }

    [HttpPost("endless-win")]
    public async Task<IActionResult> EndlessWin([FromBody] HangmanEndlessWinRequest request)
    {
        try
        {
            var result = await _hangmanService.RecordEndlessWinAsync(request.Streak);
            return Json(new { progress = result.Progress, reward = result.Reward });
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
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Hangman." });
        }
    }
}
