using CasualDate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasualDate.Controllers;

[Authorize(Roles = "Player")]
[Route("customize")]
public class CustomizeController : Controller
{
    private readonly PlayerCosmeticService _cosmeticService;

    public CustomizeController(PlayerCosmeticService cosmeticService)
    {
        _cosmeticService = cosmeticService;
    }

    [HttpGet("")]
    public IActionResult Index() => View("~/Views/Customize/Index.cshtml");

    [HttpGet("state")]
    public async Task<IActionResult> State()
    {
        try { return Json(await _cosmeticService.GetStateAsync()); }
        catch (PlayerDataUnavailableException ex) { return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message }); }
    }

    [HttpPost("equip")]
    public async Task<IActionResult> Equip([FromBody] CosmeticEquipRequest request)
    {
        try { return Json(await _cosmeticService.EquipCosmeticAsync(request.Category, request.CosmeticId)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PlayerDataUnavailableException ex) { return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message }); }
    }

    [HttpPost("companion")]
    public async Task<IActionResult> Companion([FromBody] CompanionEquipRequest request)
    {
        try { return Json(await _cosmeticService.EquipCompanionAsync(request.RewardId)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PlayerDataUnavailableException ex) { return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message }); }
    }

    [HttpPost("track")]
    public async Task<IActionResult> Track([FromBody] TrackSelectionRequest request)
    {
        try { return Json(await _cosmeticService.SelectTrackAsync(request.MediaId)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (PlayerDataUnavailableException ex) { return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message }); }
    }
}

public sealed class CosmeticEquipRequest { public string Category { get; set; } = ""; public string? CosmeticId { get; set; } }
public sealed class CompanionEquipRequest { public int? RewardId { get; set; } }
public sealed class TrackSelectionRequest { public int? MediaId { get; set; } }
