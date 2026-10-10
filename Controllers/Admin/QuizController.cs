using CasualDate.Data;
using CasualDate.Models;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/quiz")]
public class QuizController : Controller
{
    private readonly ApplicationDbContext _context;

    public QuizController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var questions = await _context.QuizQuestions
            .OrderBy(question => question.DisplayOrder)
            .ThenBy(question => question.Id)
            .ToListAsync();

        return View("~/Views/Admin/Quiz/Index.cshtml", questions);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        await LoadImageMediaAsync();
        return View("~/Views/Admin/Quiz/Edit.cshtml", CreateDefaultModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuizQuestionEditViewModel model)
    {
        NormalizeAnswers(model);
        ValidateAnswers(model);

        if (!ModelState.IsValid)
        {
            EnsureAnswerRows(model);
            await LoadImageMediaAsync();
            return View("~/Views/Admin/Quiz/Edit.cshtml", model);
        }

        var now = DateTime.UtcNow;
        var question = new QuizQuestion
        {
            QuestionText = model.QuestionText?.Trim() ?? "",
            QuestionType = model.QuestionType,
            Difficulty = model.Difficulty?.Trim() ?? "Easy",
            Points = model.Points,
            MediaId = model.MediaId,
            DisplayOrder = model.DisplayOrder,
            IsEnabled = model.IsEnabled,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.QuizQuestions.Add(question);
        await _context.SaveChangesAsync();

        AddAnswers(question.Id, model, now);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Quiz question created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var question = await _context.QuizQuestions.FindAsync(id);

        if (question is null)
        {
            return NotFound();
        }

        var answers = await _context.QuizAnswers
            .Where(answer => answer.QuizQuestionId == id)
            .OrderBy(answer => answer.DisplayOrder)
            .ThenBy(answer => answer.Id)
            .ToListAsync();

        var model = new QuizQuestionEditViewModel
        {
            Id = question.Id,
            QuestionText = question.QuestionText,
            QuestionType = question.QuestionType,
            Difficulty = question.Difficulty,
            Points = question.Points,
            MediaId = question.MediaId,
            DisplayOrder = question.DisplayOrder,
            IsEnabled = question.IsEnabled,
            CorrectAnswerIndex = Math.Max(0, answers.FindIndex(answer => answer.IsCorrect)),
            Answers = answers.Select(answer => new QuizAnswerEditViewModel
            {
                Id = answer.Id,
                AnswerText = answer.AnswerText,
                MediaId = answer.MediaId,
                DisplayOrder = answer.DisplayOrder
            }).ToList()
        };

        EnsureAnswerRows(model);
        await LoadImageMediaAsync();
        return View("~/Views/Admin/Quiz/Edit.cshtml", model);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, QuizQuestionEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        NormalizeAnswers(model);
        ValidateAnswers(model);

        if (!ModelState.IsValid)
        {
            EnsureAnswerRows(model);
            await LoadImageMediaAsync();
            return View("~/Views/Admin/Quiz/Edit.cshtml", model);
        }

        var question = await _context.QuizQuestions.FindAsync(id);

        if (question is null)
        {
            return NotFound();
        }

        question.QuestionText = model.QuestionText?.Trim() ?? "";
        question.QuestionType = model.QuestionType;
        question.Difficulty = model.Difficulty?.Trim() ?? "Easy";
        question.Points = model.Points;
        question.MediaId = model.MediaId;
        question.DisplayOrder = model.DisplayOrder;
        question.IsEnabled = model.IsEnabled;
        question.UpdatedAt = DateTime.UtcNow;

        var existingAnswers = await _context.QuizAnswers
            .Where(answer => answer.QuizQuestionId == id)
            .ToListAsync();

        SyncAnswers(id, model, existingAnswers);

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Quiz question saved.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var question = await _context.QuizQuestions.FindAsync(id);

        if (question is not null)
        {
            _context.QuizQuestions.Remove(question);
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Quiz question deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static QuizQuestionEditViewModel CreateDefaultModel()
    {
        var model = new QuizQuestionEditViewModel
        {
            Points = 10,
            IsEnabled = true
        };

        EnsureAnswerRows(model);
        return model;
    }

    private static void EnsureAnswerRows(QuizQuestionEditViewModel model)
    {
        model.Answers ??= new List<QuizAnswerEditViewModel>();

        while (model.Answers.Count < 4)
        {
            model.Answers.Add(new QuizAnswerEditViewModel
            {
                DisplayOrder = model.Answers.Count + 1
            });
        }
    }

    private static void NormalizeAnswers(QuizQuestionEditViewModel model)
    {
        EnsureAnswerRows(model);

        for (var index = 0; index < model.Answers.Count; index++)
        {
            model.Answers[index].AnswerText = model.Answers[index].AnswerText?.Trim() ?? "";
            model.Answers[index].DisplayOrder = index + 1;
        }
    }

    private void ValidateAnswers(QuizQuestionEditViewModel model)
    {
        var filledAnswers = model.Answers
            .Select((answer, index) => new { Answer = answer, Index = index })
            .Where(item => !string.IsNullOrWhiteSpace(item.Answer.AnswerText))
            .ToList();

        if (filledAnswers.Count < 2)
        {
            ModelState.AddModelError("", "Add at least two answers.");
        }

        if (model.CorrectAnswerIndex < 0 ||
            model.CorrectAnswerIndex >= model.Answers.Count ||
            string.IsNullOrWhiteSpace(model.Answers[model.CorrectAnswerIndex].AnswerText))
        {
            ModelState.AddModelError("", "Choose a correct answer that has answer text.");
        }
    }

    private void AddAnswers(int questionId, QuizQuestionEditViewModel model, DateTime now)
    {
        var filledAnswers = model.Answers
            .Select((answer, index) => new { Answer = answer, Index = index })
            .Where(item => !string.IsNullOrWhiteSpace(item.Answer.AnswerText));

        foreach (var item in filledAnswers)
        {
            _context.QuizAnswers.Add(new QuizAnswer
            {
                QuizQuestionId = questionId,
                AnswerText = item.Answer.AnswerText,
                MediaId = item.Answer.MediaId,
                IsCorrect = item.Index == model.CorrectAnswerIndex,
                DisplayOrder = item.Index + 1,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
    }

    private void SyncAnswers(int questionId, QuizQuestionEditViewModel model, List<QuizAnswer> existingAnswers)
    {
        var now = DateTime.UtcNow;

        foreach (var item in model.Answers.Select((answer, index) => new { Answer = answer, Index = index }))
        {
            var answerText = item.Answer.AnswerText?.Trim() ?? "";
            var existingAnswer = existingAnswers.FirstOrDefault(answer => answer.Id == item.Answer.Id);

            if (string.IsNullOrWhiteSpace(answerText))
            {
                if (existingAnswer is not null)
                {
                    _context.QuizAnswers.Remove(existingAnswer);
                }

                continue;
            }

            if (existingAnswer is null)
            {
                _context.QuizAnswers.Add(new QuizAnswer
                {
                    QuizQuestionId = questionId,
                    AnswerText = answerText,
                    MediaId = item.Answer.MediaId,
                    IsCorrect = item.Index == model.CorrectAnswerIndex,
                    DisplayOrder = item.Index + 1,
                    CreatedAt = now,
                    UpdatedAt = now
                });

                continue;
            }

            existingAnswer.AnswerText = answerText;
            existingAnswer.MediaId = item.Answer.MediaId;
            existingAnswer.IsCorrect = item.Index == model.CorrectAnswerIndex;
            existingAnswer.DisplayOrder = item.Index + 1;
            existingAnswer.UpdatedAt = now;
        }
    }

    private async Task LoadImageMediaAsync()
    {
        var images = await _context.UploadedMedia
            .Where(media => media.MediaType == MediaType.Image)
            .OrderBy(media => media.DisplayName)
            .ThenBy(media => media.OriginalFileName)
            .Select(media => new SelectListItem
            {
                Value = media.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(media.DisplayName)
                    ? media.OriginalFileName
                    : media.DisplayName
            })
            .ToListAsync();

        images.Insert(0, new SelectListItem { Value = "", Text = "No image" });
        ViewBag.ImageMediaItems = images;
    }
}
