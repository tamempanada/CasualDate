using System.Data.Common;
using CasualDate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CasualDate.Controllers;

[Route("game-rewards")]
public class GameRewardsController : Controller
{
    private readonly GameRewardService _gameRewardService;
    private readonly ILogger<GameRewardsController> _logger;

    public GameRewardsController(GameRewardService gameRewardService, ILogger<GameRewardsController> logger)
    {
        _gameRewardService = gameRewardService;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] GameAttemptStartRequest request)
    {
        try
        {
            var result = await _gameRewardService.StartAttemptAsync(request.GameSlug?.Trim().ToLowerInvariant() ?? "");
            return result.Succeeded ? Json(result) : BadRequest(new { message = result.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Player login required." });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for game rewards." });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Game reward attempt could not be completed.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.GetBaseException().Message });
        }
    }

    [HttpPost("cake/complete")]
    public Task<IActionResult> CompleteCake([FromBody] CakeCompletionRequest request) => CompleteAsync(() => _gameRewardService.CompleteCakeAsync(request));

    [HttpPost("perfect-date/complete")]
    public Task<IActionResult> CompletePerfectDate([FromBody] PerfectDateCompletionRequest request) => CompleteAsync(() => _gameRewardService.CompletePerfectDateAsync(request));

    [HttpPost("photobooth/capture")]
    public async Task<IActionResult> RegisterPhotoboothCapture([FromBody] PhotoboothCaptureRequest request)
    {
        try
        {
            var result = await _gameRewardService.RegisterPhotoboothCaptureAsync(request.AttemptToken);
            return result.Succeeded ? Json(result) : BadRequest(new { message = result.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Player login required." });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for game rewards." });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Game reward attempt could not be completed.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.GetBaseException().Message });
        }
    }

    [HttpPost("photobooth/complete")]
    public Task<IActionResult> CompletePhotobooth([FromBody] PhotoboothCompletionRequest request) => CompleteAsync(() => _gameRewardService.CompletePhotoboothAsync(request.AttemptToken));

    private async Task<IActionResult> CompleteAsync(Func<Task<GameRewardCompletionResult>> action)
    {
        try
        {
            var result = await action();
            return result.Succeeded ? Json(result) : BadRequest(new { message = result.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Player login required." });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for game rewards." });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }
}

public sealed class GameAttemptStartRequest
{
    public string? GameSlug { get; set; }
}

public sealed class PhotoboothCaptureRequest
{
    public string AttemptToken { get; set; } = "";
}

public sealed class PhotoboothCompletionRequest
{
    public string AttemptToken { get; set; } = "";
}
