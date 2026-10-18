using System.Data.Common;
using CasualDate.Data;
using CasualDate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers;

[Route("quiz")]
public class QuizController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly TokenWalletService _tokenWalletService;

    public QuizController(ApplicationDbContext context, TokenWalletService tokenWalletService)
    {
        _context = context;
        _tokenWalletService = tokenWalletService;
    }

    [HttpPost("answer")]
    public async Task<IActionResult> Answer([FromBody] QuizAnswerRequest request)
    {
        var answer = await _context.QuizAnswers
            .FirstOrDefaultAsync(item => item.Id == request.AnswerId && item.QuizQuestionId == request.QuestionId);

        if (answer is null)
        {
            return BadRequest(new { message = "That answer is no longer available." });
        }

        var question = await _context.QuizQuestions
            .FirstOrDefaultAsync(item => item.Id == answer.QuizQuestionId && item.IsEnabled);
        if (question is null)
        {
            return BadRequest(new { message = "That question is no longer available." });
        }

        try
        {
            var rewardKey = $"quiz:question:{answer.QuizQuestionId}";
            if (answer.IsCorrect)
            {
                await _tokenWalletService.EnsureRewardSettingAsync(rewardKey, $"Quiz question: {question.QuestionText}", 1);
            }

            var reward = answer.IsCorrect
                ? await _tokenWalletService.AwardCompletionAsync(rewardKey)
                : new TokenAwardResult(false, 0, (await _tokenWalletService.GetSnapshotAsync()).Balance, "Incorrect answer.");

            return Json(new
            {
                isCorrect = answer.IsCorrect,
                points = answer.IsCorrect ? question.Points : 0,
                reward
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Player login required." });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database migration required for Quiz rewards." });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }
}

public sealed class QuizAnswerRequest
{
    public int QuestionId { get; set; }

    public int AnswerId { get; set; }
}
