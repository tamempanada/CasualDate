using System.Data.Common;
using CasualDate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CasualDate.Controllers;

[Route("scrapbook")]
public class ScrapbookController : Controller
{
    private readonly ScrapbookService _scrapbookService;

    public ScrapbookController(ScrapbookService scrapbookService)
    {
        _scrapbookService = scrapbookService;
    }

    [HttpGet("pages")]
    public async Task<IActionResult> Pages()
    {
        try
        {
            var pages = await _scrapbookService.GetPagesAsync();
            return Json(new { pages });
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
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Scrapbook." });
        }
    }

    [HttpPost("save")]
    public async Task<IActionResult> Save([FromBody] ScrapbookPageSaveRequest request)
    {
        try
        {
            var page = await _scrapbookService.SavePageAsync(request);
            return Json(new { page, message = "Saved." });
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
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Scrapbook." });
        }
    }
}
