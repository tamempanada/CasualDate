using CasualDate.Data;
using CasualDate.Models;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;

namespace CasualDate.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/media")]
public class MediaController : Controller
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private static readonly string[] AllowedAudioExtensions = [".mp3", ".wav", ".ogg", ".m4a"];

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public MediaController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        await PopulateSelectListsAsync();

        var media = await _context.UploadedMedia
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();

        ViewBag.MediaUsages = await BuildMediaUsageMapAsync(media.Select(item => item.Id));
        ViewBag.FileExists = media.ToDictionary(item => item.Id, item => MediaFileExists(item));
        ViewBag.UploadModel = new MediaUploadViewModel();
        return View("~/Views/Admin/Media/Index.cshtml", media);
    }

    [HttpPost("upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(MediaUploadViewModel model)
    {
        if (model.File is null || model.File.Length == 0)
        {
            TempData["StatusKind"] = "error";
            TempData["StatusMessage"] = "Choose a file before uploading.";
            return RedirectToAction(nameof(Index));
        }

        var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
        var isImage = AllowedImageExtensions.Contains(extension);
        var isAudio = AllowedAudioExtensions.Contains(extension);

        if (!isImage && !isAudio)
        {
            TempData["StatusKind"] = "error";
            TempData["StatusMessage"] = "Unsupported file type.";
            return RedirectToAction(nameof(Index));
        }

        var maxFileSize = isAudio ? 20 * 1024 * 1024 : 8 * 1024 * 1024;

        if (model.File.Length > maxFileSize)
        {
            TempData["StatusKind"] = "error";
            TempData["StatusMessage"] = isAudio
                ? "Audio is too large. Keep uploads under 20 MB for now."
                : "Image is too large. Keep uploads under 8 MB for now.";
            return RedirectToAction(nameof(Index));
        }

        var folderName = isImage ? "images" : "audio";
        var uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(uploadRoot);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(uploadRoot, storedFileName);
        var displayName = string.IsNullOrWhiteSpace(model.DisplayName)
            ? Path.GetFileNameWithoutExtension(model.File.FileName)
            : model.DisplayName.Trim();
        var category = model.Category?.Trim() ?? "";

        await using (var stream = System.IO.File.Create(absolutePath))
        {
            await model.File.CopyToAsync(stream);
        }

        int? width = null;
        int? height = null;

        if (isImage)
        {
            await using var imageStream = System.IO.File.OpenRead(absolutePath);
            var imageInfo = await Image.IdentifyAsync(imageStream);

            if (imageInfo is null)
            {
                System.IO.File.Delete(absolutePath);
                TempData["StatusKind"] = "error";
                TempData["StatusMessage"] = "The uploaded image could not be read.";
                return RedirectToAction(nameof(Index));
            }

            width = imageInfo.Width;
            height = imageInfo.Height;

            if (width > 5000 || height > 5000)
            {
                System.IO.File.Delete(absolutePath);
                TempData["StatusKind"] = "error";
                TempData["StatusMessage"] = "Image dimensions are too large. Keep images under 5000 x 5000.";
                return RedirectToAction(nameof(Index));
            }
        }

        _context.UploadedMedia.Add(new UploadedMedia
        {
            MiniGameId = model.MiniGameId,
            OriginalFileName = Path.GetFileName(model.File.FileName),
            StoredFileName = storedFileName,
            FilePath = $"/uploads/{folderName}/{storedFileName}",
            ContentType = model.File.ContentType ?? "",
            MediaType = isImage ? MediaType.Image : MediaType.Audio,
            Category = category,
            DisplayName = displayName,
            FileSizeBytes = model.File.Length,
            Width = width,
            Height = height
        });

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Media uploaded.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var media = await _context.UploadedMedia.FindAsync(id);

        if (media is null)
        {
            TempData["StatusKind"] = "error";
            TempData["StatusMessage"] = "That media item no longer exists.";
            return RedirectToAction(nameof(Index));
        }

        var usages = await GetMediaUsagesAsync(id);

        if (usages.Count > 0)
        {
            TempData["StatusKind"] = "error";
            TempData["StatusMessage"] = $"This media is currently being used by {string.Join(", ", usages.Take(4))}{(usages.Count > 4 ? ", and more" : "")}. Replace or remove it there before deleting.";
            return RedirectToAction(nameof(Index));
        }

        var fileDeleted = TryDeletePhysicalFileIfPresent(media);

        _context.UploadedMedia.Remove(media);
        await _context.SaveChangesAsync();

        if (fileDeleted)
        {
            TempData["StatusMessage"] = "Media deleted.";
        }
        else
        {
            TempData["StatusKind"] = "error";
            TempData["StatusMessage"] = "Media record deleted, but the stored file could not be removed. It may be open in another app or already unavailable.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("update/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, MediaEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var media = await _context.UploadedMedia.FindAsync(id);

        if (media is null)
        {
            return NotFound();
        }

        media.DisplayName = string.IsNullOrWhiteSpace(model.DisplayName)
            ? Path.GetFileNameWithoutExtension(media.OriginalFileName)
            : model.DisplayName.Trim();
        media.Category = model.Category?.Trim() ?? "";
        media.MiniGameId = model.MiniGameId;

        if (media.MediaType == MediaType.Audio)
        {
            media.AlbumCoverMediaId = await GetValidAlbumCoverIdAsync(model.AlbumCoverMediaId);
        }
        else
        {
            media.AlbumCoverMediaId = null;
        }

        media.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Media details saved.";

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateSelectListsAsync()
    {
        var games = await _context.MiniGames
            .OrderBy(game => game.DisplayOrder)
            .Select(game => new SelectListItem(game.Name, game.Id.ToString()))
            .ToListAsync();

        games.Insert(0, new SelectListItem("General media", ""));
        ViewBag.MiniGames = games;

        var covers = await _context.UploadedMedia
            .Where(media => media.MediaType == MediaType.Image)
            .OrderBy(media => media.DisplayName)
            .Select(media => new SelectListItem(media.DisplayName, media.Id.ToString()))
            .ToListAsync();

        covers.Insert(0, new SelectListItem("No album cover", ""));
        ViewBag.AlbumCovers = covers;
    }

    private async Task<int?> GetValidAlbumCoverIdAsync(int? albumCoverMediaId)
    {
        if (albumCoverMediaId is null)
        {
            return null;
        }

        var exists = await _context.UploadedMedia
            .AnyAsync(media => media.Id == albumCoverMediaId && media.MediaType == MediaType.Image);

        return exists ? albumCoverMediaId : null;
    }

    private async Task<Dictionary<int, List<string>>> BuildMediaUsageMapAsync(IEnumerable<int> mediaIds)
    {
        var ids = mediaIds.Distinct().ToList();
        var usageMap = ids.ToDictionary(id => id, _ => new List<string>());

        if (ids.Count == 0)
        {
            return usageMap;
        }

        var settings = await _context.GameSettings
            .Select(setting => new { setting.BackgroundMediaId, setting.MusicMediaId })
            .ToListAsync();

        foreach (var setting in settings)
        {
            AddUsage(usageMap, setting.BackgroundMediaId, "Site background");
            AddUsage(usageMap, setting.MusicMediaId, "Background music");
        }

        var albumUsages = await _context.UploadedMedia
            .Where(media => media.AlbumCoverMediaId.HasValue && ids.Contains(media.AlbumCoverMediaId.Value))
            .Select(media => new { MediaId = media.AlbumCoverMediaId, media.DisplayName, media.OriginalFileName })
            .ToListAsync();

        foreach (var usage in albumUsages)
        {
            AddUsage(usageMap, usage.MediaId, $"Album cover for {DisplayMediaName(usage.DisplayName, usage.OriginalFileName)}");
        }

        var messageUsages = await _context.Messages
            .Where(message => message.MediaId.HasValue && ids.Contains(message.MediaId.Value))
            .Select(message => new { message.MediaId, message.Title, message.Key })
            .ToListAsync();

        foreach (var usage in messageUsages)
        {
            AddUsage(usageMap, usage.MediaId, $"Message: {DisplayText(usage.Title, usage.Key)}");
        }

        var resultUsages = await _context.ResultRanges
            .Where(range => range.MediaId.HasValue && ids.Contains(range.MediaId.Value))
            .Select(range => new { range.MediaId, range.Title })
            .ToListAsync();

        foreach (var usage in resultUsages)
        {
            AddUsage(usageMap, usage.MediaId, $"Ending image: {DisplayText(usage.Title, "Result")}");
        }

        var dateUsages = await _context.DateOptions
            .Where(option => option.MediaId.HasValue && ids.Contains(option.MediaId.Value))
            .Select(option => new { option.MediaId, option.Name })
            .ToListAsync();

        foreach (var usage in dateUsages)
        {
            AddUsage(usageMap, usage.MediaId, $"Perfect Date choice: {DisplayText(usage.Name, "Choice")}");
        }

        var cakeUsages = await _context.CakeItems
            .Where(item => item.MediaId.HasValue && ids.Contains(item.MediaId.Value))
            .Select(item => new { item.MediaId, item.Name, item.ItemType })
            .ToListAsync();

        foreach (var usage in cakeUsages)
        {
            AddUsage(usageMap, usage.MediaId, $"Cake {usage.ItemType}: {DisplayText(usage.Name, "Item")}");
        }

        var gachaUsages = await _context.GachaRewards
            .Where(reward => reward.MediaId.HasValue && ids.Contains(reward.MediaId.Value))
            .Select(reward => new { reward.MediaId, reward.Name, reward.RewardType })
            .ToListAsync();

        foreach (var usage in gachaUsages)
        {
            AddUsage(usageMap, usage.MediaId, $"Gacha {usage.RewardType}: {DisplayText(usage.Name, "Reward")}");
        }

        var quizQuestionUsages = await _context.QuizQuestions
            .Where(question => question.MediaId.HasValue && ids.Contains(question.MediaId.Value))
            .Select(question => new { question.MediaId, question.QuestionText })
            .ToListAsync();

        foreach (var usage in quizQuestionUsages)
        {
            AddUsage(usageMap, usage.MediaId, $"Quiz question: {DisplayText(usage.QuestionText, "Question")}");
        }

        var quizAnswerUsages = await _context.QuizAnswers
            .Where(answer => answer.MediaId.HasValue && ids.Contains(answer.MediaId.Value))
            .Select(answer => new { answer.MediaId, answer.AnswerText })
            .ToListAsync();

        foreach (var usage in quizAnswerUsages)
        {
            AddUsage(usageMap, usage.MediaId, $"Quiz answer: {DisplayText(usage.AnswerText, "Answer")}");
        }

        return usageMap;
    }

    private async Task<List<string>> GetMediaUsagesAsync(int mediaId)
    {
        var usageMap = await BuildMediaUsageMapAsync([mediaId]);
        return usageMap.TryGetValue(mediaId, out var usages) ? usages : new List<string>();
    }

    private static void AddUsage(Dictionary<int, List<string>> usageMap, int? mediaId, string usage)
    {
        if (mediaId.HasValue && usageMap.TryGetValue(mediaId.Value, out var usages) && !usages.Contains(usage))
        {
            usages.Add(usage);
        }
    }

    private static string DisplayMediaName(string displayName, string originalFileName)
    {
        return DisplayText(displayName, originalFileName);
    }

    private static string DisplayText(string value, string fallback)
    {
        var text = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return text.Length <= 48 ? text : $"{text[..45]}...";
    }

    private bool MediaFileExists(UploadedMedia media)
    {
        var absolutePath = GetMediaAbsolutePath(media);
        return absolutePath is not null && System.IO.File.Exists(absolutePath);
    }

    private bool TryDeletePhysicalFileIfPresent(UploadedMedia media)
    {
        var absolutePath = GetMediaAbsolutePath(media);

        if (absolutePath is null || !System.IO.File.Exists(absolutePath))
        {
            return true;
        }

        try
        {
            System.IO.File.Delete(absolutePath);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private string? GetMediaAbsolutePath(UploadedMedia media)
    {
        if (string.IsNullOrWhiteSpace(media.FilePath))
        {
            return null;
        }

        var relativePath = media.FilePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, relativePath));
        var webRootPath = Path.GetFullPath(_environment.WebRootPath);

        return absolutePath.StartsWith(webRootPath, StringComparison.OrdinalIgnoreCase)
            ? absolutePath
            : null;
    }
}
