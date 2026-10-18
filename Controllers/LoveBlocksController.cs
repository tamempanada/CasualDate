using System.Data.Common;
using CasualDate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CasualDate.Controllers;

[Route("love-blocks")]
public class LoveBlocksController : Controller
{
    private readonly LoveBlocksService _service;
    public LoveBlocksController(LoveBlocksService service) => _service = service;

    [HttpGet("state")]
    public async Task<IActionResult> State() => await ExecuteAsync(() => _service.StartAsync());

    [HttpPost("start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start() => await ExecuteAsync(() => _service.StartAsync(restart: true));

    [HttpPost("place")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Place([FromBody] LoveBlocksMoveRequest request) =>
        await ExecuteAsync(() => _service.PlaceAsync(request.RunToken, request.PieceIndex, request.Row, request.Column, request.RequestId));

    private async Task<IActionResult> ExecuteAsync(Func<Task<LoveBlocksState>> action)
    {
        try { return Json(new { game = await action() }); }
        catch (UnauthorizedAccessException) { return Unauthorized(new { message = "Player login required." }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PlayerDataUnavailableException ex) { return StatusCode(503, new { message = ex.Message }); }
        catch (DbException) { return StatusCode(503, new { message = "The Love Blocks database migration is required." }); }
    }
}

public sealed class LoveBlocksMoveRequest
{
    public string RunToken { get; set; } = "";
    public int PieceIndex { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public string RequestId { get; set; } = "";
}
