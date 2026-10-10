using CasualDate.Data;
using CasualDate.Models;
using CasualDate.ViewModels.Player;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers;

public class GameController : Controller
{
    private readonly ApplicationDbContext _context;

    public GameController(ApplicationDbContext context)
    {
        _context = context;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Photobooth()
    {
        var settings = await GetSettingsAsync();

        return View(new PhotoboothGameViewModel
        {
            CompletionHeading = settings.PhotoboothCompletionHeading,
            CompletionSubheading = settings.PhotoboothCompletionSubheading
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Cake()
    {
        await EnsureDefaultCakeItemsAsync();
        await EnsureDefaultResultRangesAsync(
            "cake",
            new[]
            {
                new DefaultResultRange(0, 59, "Bakery Panic", "Hope it sells well....", 1),
                new DefaultResultRange(60, 84, "Pretty Good", "Pretty good.", 2),
                new DefaultResultRange(85, 100, "Perfect Batch", "This batch is as perfect as you.", 3)
            });

        var items = await _context.CakeItems
            .Where(item => item.IsEnabled)
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Name)
            .ToListAsync();
        var mediaPaths = await GetImagePathsAsync(items.Select(item => item.MediaId));

        var resultRanges = await GetResultRangesAsync("cake");

        return View(new CakeGameViewModel
        {
            Bases = MapCakeItems(items, CakeItemType.Base, mediaPaths),
            Frostings = MapCakeItems(items, CakeItemType.Frosting, mediaPaths),
            Toppings = MapCakeItems(items, CakeItemType.Topping, mediaPaths),
            Decorations = MapCakeItems(items, CakeItemType.Decoration, mediaPaths),
            ResultRanges = resultRanges
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> PerfectDate()
    {
        await EnsureDefaultDateOptionsAsync();
        await EnsureDefaultResultRangesAsync(
            "perfect-date",
            new[]
            {
                new DefaultResultRange(0, 79, "Bad Ending", "The date ended awkwardly and both of you went home wondering what went wrong.", 1),
                new DefaultResultRange(80, 91, "Normal Ending", "It wasn't perfect, but you both had a nice time together.", 2),
                new DefaultResultRange(92, 120, "Good Ending", "The date couldn't have gone better. It became one of those moments you both wanted to remember.", 3)
            });

        var categories = await _context.DateCategories
            .Where(category => category.IsEnabled)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ToListAsync();

        var categoryIds = categories.Select(category => category.Id).ToList();
        var options = await _context.DateOptions
            .Where(option => option.IsEnabled && categoryIds.Contains(option.DateCategoryId))
            .OrderBy(option => option.DisplayOrder)
            .ThenBy(option => option.Name)
            .ToListAsync();
        var mediaPaths = await GetImagePathsAsync(options.Select(option => option.MediaId));

        var ranges = await GetResultRangesAsync("perfect-date");

        return View(new PerfectDateGameViewModel
        {
            Categories = categories
                .Select(category => new DateCategoryChoiceViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    Options = options
                        .Where(option => option.DateCategoryId == category.Id)
                        .Select(option => new DateOptionChoiceViewModel
                        {
                            Id = option.Id,
                            Name = option.Name,
                            Description = option.Description,
                            Score = option.Score,
                            ImagePath = GetImagePath(mediaPaths, option.MediaId)
                        })
                        .ToList()
                })
                .Where(category => category.Options.Count > 0)
                .ToList(),
            ResultRanges = ranges
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Quiz()
    {
        await EnsureDefaultQuizQuestionsAsync();
        await EnsureDefaultResultRangesAsync(
            "quiz",
            new[]
            {
                new DefaultResultRange(0, 24, "Bad Ending", "A cute attempt, but this absolutely deserves a playful rematch.", 1),
                new DefaultResultRange(25, 39, "Normal Ending", "Not perfect, but still sweet enough to count.", 2),
                new DefaultResultRange(40, 100, "Good Ending", "Almost suspiciously good. You were paying attention.", 3)
            });

        var questions = await _context.QuizQuestions
            .Where(question => question.IsEnabled)
            .OrderBy(question => question.DisplayOrder)
            .ThenBy(question => question.Id)
            .ToListAsync();
        var mediaPaths = await GetImagePathsAsync(questions.Select(question => question.MediaId));

        var questionIds = questions.Select(question => question.Id).ToList();
        var answers = await _context.QuizAnswers
            .Where(answer => questionIds.Contains(answer.QuizQuestionId))
            .OrderBy(answer => answer.DisplayOrder)
            .ThenBy(answer => answer.Id)
            .ToListAsync();
        var answerMediaPaths = await GetImagePathsAsync(answers.Select(answer => answer.MediaId));

        var ranges = await GetResultRangesAsync("quiz");

        return View(new QuizGameViewModel
        {
            Questions = questions
                .Select(question => new QuizQuestionViewModel
                {
                    Id = question.Id,
                    QuestionText = question.QuestionText,
                    Difficulty = question.Difficulty,
                    Points = question.Points,
                    ImagePath = GetImagePath(mediaPaths, question.MediaId),
                    Answers = answers
                        .Where(answer => answer.QuizQuestionId == question.Id)
                        .Select(answer => new QuizAnswerChoiceViewModel
                        {
                            Id = answer.Id,
                            AnswerText = answer.AnswerText,
                            ImagePath = GetImagePath(answerMediaPaths, answer.MediaId),
                            IsCorrect = answer.IsCorrect
                        })
                        .ToList()
                })
                .Where(question => question.Answers.Count > 0)
                .ToList(),
            ResultRanges = ranges
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Final()
    {
        var settings = await GetSettingsAsync();
        var requiredGameSlugs = await _context.MiniGames
            .Where(game => game.IsEnabled)
            .OrderBy(game => game.DisplayOrder)
            .Select(game => game.Slug)
            .ToListAsync();

        return View(new FinalViewModel
        {
            GameTitle = settings.GameTitle,
            FinalTitle = settings.FinalTitle,
            FinalMessage = settings.FinalMessage,
            PrimaryColor = settings.PrimaryColor,
            SecondaryColor = settings.SecondaryColor,
            RequiredGameSlugs = requiredGameSlugs
        });
    }

    private async Task<GameSetting> GetSettingsAsync()
    {
        return await _context.GameSettings
            .OrderBy(setting => setting.Id)
            .FirstOrDefaultAsync()
            ?? new GameSetting();
    }

    private static List<CakeItemChoiceViewModel> MapCakeItems(
        List<CakeItem> items,
        CakeItemType itemType,
        Dictionary<int, string> mediaPaths)
    {
        return items
            .Where(item => item.ItemType == itemType)
            .Select(item => new CakeItemChoiceViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Score = item.Score,
                Color = GetCakeItemColor(item),
                ImagePath = GetImagePath(mediaPaths, item.MediaId)
            })
            .ToList();
    }

    private async Task<Dictionary<int, string>> GetImagePathsAsync(IEnumerable<int?> mediaIds)
    {
        var ids = mediaIds
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<int, string>();
        }

        return await _context.UploadedMedia
            .Where(media => ids.Contains(media.Id)
                && (media.MediaType == MediaType.Image || EF.Functions.Like(media.ContentType, "image/%")))
            .ToDictionaryAsync(media => media.Id, media => media.FilePath);
    }

    private static string? GetImagePath(Dictionary<int, string> mediaPaths, int? mediaId)
    {
        return mediaId.HasValue && mediaPaths.TryGetValue(mediaId.Value, out var imagePath)
            ? imagePath
            : null;
    }

    private async Task<List<DateResultRangeViewModel>> GetResultRangesAsync(string miniGameSlug)
    {
        var game = await _context.MiniGames
            .FirstOrDefaultAsync(item => item.Slug == miniGameSlug);

        if (game is null)
        {
            return [];
        }

        var ranges = await _context.ResultRanges
            .Where(range => range.MiniGameId == game.Id)
            .OrderBy(range => range.DisplayOrder)
            .ThenBy(range => range.MinScore)
            .ToListAsync();
        var mediaPaths = await GetImagePathsAsync(ranges.Select(range => range.MediaId));

        return ranges
            .Select(range => new DateResultRangeViewModel
            {
                MinScore = range.MinScore,
                MaxScore = range.MaxScore,
                Title = range.Title,
                Message = range.Message,
                ImagePath = GetImagePath(mediaPaths, range.MediaId) ?? ""
            })
            .ToList();
    }

    private static string GetCakeItemColor(CakeItem item)
    {
        var text = $"{item.Name} {item.Description}".ToLowerInvariant();

        if (text.Contains("chocolate"))
        {
            return "#7b4b3a";
        }

        if (text.Contains("strawberry") || text.Contains("berry"))
        {
            return "#ef6f8f";
        }

        if (text.Contains("vanilla") || text.Contains("cream"))
        {
            return "#f8e7ba";
        }

        if (text.Contains("red"))
        {
            return "#b64048";
        }

        if (text.Contains("sprinkle"))
        {
            return "#7bb8a5";
        }

        if (text.Contains("flower"))
        {
            return "#d98ac1";
        }

        if (text.Contains("candle"))
        {
            return "#f6c95f";
        }

        return item.ItemType switch
        {
            CakeItemType.Base => "#d9a066",
            CakeItemType.Frosting => "#ffe1e8",
            CakeItemType.Topping => "#c85f75",
            CakeItemType.Decoration => "#7bb8a5",
            _ => "#f6c95f"
        };
    }

    private async Task EnsureDefaultCakeItemsAsync()
    {
        if (await _context.CakeItems.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var items = new List<CakeItem>
        {
            new() { ItemType = CakeItemType.Base, Name = "Chocolate", Description = "Rich and dramatic in the best way.", Score = 24, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { ItemType = CakeItemType.Base, Name = "Vanilla", Description = "Soft, classic, and quietly perfect.", Score = 20, DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { ItemType = CakeItemType.Base, Name = "Strawberry", Description = "Sweet, pink, and very date-coded.", Score = 26, DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },
            new() { ItemType = CakeItemType.Base, Name = "Red Velvet", Description = "A little fancy without trying too hard.", Score = 28, DisplayOrder = 4, CreatedAt = now, UpdatedAt = now },

            new() { ItemType = CakeItemType.Frosting, Name = "Chocolate Frosting", Description = "For serious chocolate commitment.", Score = 22, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { ItemType = CakeItemType.Frosting, Name = "Vanilla Cream", Description = "Smooth, sweet, and dependable.", Score = 20, DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { ItemType = CakeItemType.Frosting, Name = "Strawberry Frosting", Description = "A soft pink layer of affection.", Score = 24, DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },
            new() { ItemType = CakeItemType.Frosting, Name = "Cream Cheese", Description = "A tiny upgrade with main-character energy.", Score = 26, DisplayOrder = 4, CreatedAt = now, UpdatedAt = now },

            new() { ItemType = CakeItemType.Topping, Name = "Sprinkles", Description = "Joy, but crunchy.", Score = 18, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { ItemType = CakeItemType.Topping, Name = "Strawberries", Description = "Cute and suspiciously romantic.", Score = 25, DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { ItemType = CakeItemType.Topping, Name = "Cherries", Description = "The photogenic option.", Score = 20, DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },
            new() { ItemType = CakeItemType.Topping, Name = "Cookies", Description = "Chaos, but delicious.", Score = 21, DisplayOrder = 4, CreatedAt = now, UpdatedAt = now },

            new() { ItemType = CakeItemType.Decoration, Name = "Hearts", Description = "Obvious, effective, correct.", Score = 25, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { ItemType = CakeItemType.Decoration, Name = "Flowers", Description = "Soft and pretty without being loud.", Score = 22, DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { ItemType = CakeItemType.Decoration, Name = "Candles", Description = "For making wishes and pretending not to.", Score = 19, DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },
            new() { ItemType = CakeItemType.Decoration, Name = "Initials", Description = "Personalized in the most dangerous way.", Score = 28, DisplayOrder = 4, CreatedAt = now, UpdatedAt = now }
        };

        _context.CakeItems.AddRange(items);
        await _context.SaveChangesAsync();
    }

    private async Task EnsureDefaultDateOptionsAsync()
    {
        if (await _context.DateCategories.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var categories = new List<DateCategory>
        {
            new() { Name = "Where should we go?", Description = "Pick the place for the date.", DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { Name = "What should we eat?", Description = "Choose the food mood.", DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { Name = "What should we do?", Description = "Choose the main activity.", DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },
            new() { Name = "How should it end?", Description = "Pick the final moment.", DisplayOrder = 4, CreatedAt = now, UpdatedAt = now }
        };

        _context.DateCategories.AddRange(categories);
        await _context.SaveChangesAsync();

        var options = new List<DateOption>
        {
            new() { DateCategoryId = categories[0].Id, Name = "Cozy Cafe", Description = "Warm drinks, soft seats, and talking too long.", Score = 22, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { DateCategoryId = categories[0].Id, Name = "Arcade", Description = "Games, tiny rivalries, and dramatic wins.", Score = 24, DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { DateCategoryId = categories[0].Id, Name = "Park Walk", Description = "Fresh air, hand holding, and slow wandering.", Score = 20, DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },
            new() { DateCategoryId = categories[0].Id, Name = "Movie Night", Description = "Snacks, blanket, and picking the best seats.", Score = 18, DisplayOrder = 4, CreatedAt = now, UpdatedAt = now },

            new() { DateCategoryId = categories[1].Id, Name = "Pizza", Description = "Easy, happy, impossible to overthink.", Score = 18, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { DateCategoryId = categories[1].Id, Name = "Pasta", Description = "A little fancy, a little messy, very worth it.", Score = 24, DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { DateCategoryId = categories[1].Id, Name = "Dessert First", Description = "Correct priorities, honestly.", Score = 25, DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },
            new() { DateCategoryId = categories[1].Id, Name = "Street Food", Description = "Casual, fun, and full of tiny discoveries.", Score = 21, DisplayOrder = 4, CreatedAt = now, UpdatedAt = now },

            new() { DateCategoryId = categories[2].Id, Name = "Photo Challenge", Description = "Take cute pictures and pretend it is not a whole production.", Score = 24, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { DateCategoryId = categories[2].Id, Name = "Build a Playlist", Description = "Trade songs and judge each other's taste lovingly.", Score = 22, DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { DateCategoryId = categories[2].Id, Name = "Board Game", Description = "Strategy, laughter, and suspiciously serious rules.", Score = 19, DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },
            new() { DateCategoryId = categories[2].Id, Name = "Sunset Walk", Description = "Simple, pretty, and unfairly effective.", Score = 25, DisplayOrder = 4, CreatedAt = now, UpdatedAt = now },

            new() { DateCategoryId = categories[3].Id, Name = "Tiny Gift", Description = "A sweet little surprise at the end.", Score = 24, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { DateCategoryId = categories[3].Id, Name = "Long Hug", Description = "Soft ending, excellent rating.", Score = 23, DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { DateCategoryId = categories[3].Id, Name = "One More Snack", Description = "Because the date clearly needs an encore.", Score = 19, DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },
            new() { DateCategoryId = categories[3].Id, Name = "Goodnight Message", Description = "Sent before either person stops smiling.", Score = 22, DisplayOrder = 4, CreatedAt = now, UpdatedAt = now }
        };

        _context.DateOptions.AddRange(options);

        await _context.SaveChangesAsync();
    }

    private async Task EnsureDefaultResultRangesAsync(string miniGameSlug, IEnumerable<DefaultResultRange> defaultRanges)
    {
        var game = await _context.MiniGames.FirstOrDefaultAsync(item => item.Slug == miniGameSlug);

        if (game is null || await _context.ResultRanges.AnyAsync(range => range.MiniGameId == game.Id))
        {
            return;
        }

        var now = DateTime.UtcNow;
        _context.ResultRanges.AddRange(defaultRanges.Select(range => new ResultRange
        {
            MiniGameId = game.Id,
            MinScore = range.MinScore,
            MaxScore = range.MaxScore,
            Title = range.Title,
            Message = range.Message,
            DisplayOrder = range.DisplayOrder,
            CreatedAt = now,
            UpdatedAt = now
        }));

        await _context.SaveChangesAsync();
    }

    private async Task EnsureDefaultQuizQuestionsAsync()
    {
        if (await _context.QuizQuestions.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var questions = new List<QuizQuestion>
        {
            new()
            {
                QuestionText = "What is the safest answer when asked if we should get dessert?",
                Difficulty = "Easy",
                Points = 10,
                DisplayOrder = 1,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                QuestionText = "What kind of date energy is this game going for?",
                Difficulty = "Easy",
                Points = 10,
                DisplayOrder = 2,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                QuestionText = "True or false: tiny thoughtful things count as romance.",
                QuestionType = QuizQuestionType.TrueFalse,
                Difficulty = "Easy",
                Points = 10,
                DisplayOrder = 3,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                QuestionText = "Pick the best reaction to a cute photo together.",
                Difficulty = "Medium",
                Points = 10,
                DisplayOrder = 4,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                QuestionText = "What is the correct amount of effort for a special person?",
                Difficulty = "Medium",
                Points = 10,
                DisplayOrder = 5,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        _context.QuizQuestions.AddRange(questions);
        await _context.SaveChangesAsync();

        var answers = new List<QuizAnswer>
        {
            new() { QuizQuestionId = questions[0].Id, AnswerText = "Yes, dessert is important.", IsCorrect = true, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { QuizQuestionId = questions[0].Id, AnswerText = "Maybe later.", DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { QuizQuestionId = questions[0].Id, AnswerText = "No dessert.", DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },

            new() { QuizQuestionId = questions[1].Id, AnswerText = "Playful, sweet, and personal.", IsCorrect = true, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { QuizQuestionId = questions[1].Id, AnswerText = "Extremely serious business paperwork.", DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { QuizQuestionId = questions[1].Id, AnswerText = "A random math exam.", DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },

            new() { QuizQuestionId = questions[2].Id, AnswerText = "True", IsCorrect = true, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { QuizQuestionId = questions[2].Id, AnswerText = "False", DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },

            new() { QuizQuestionId = questions[3].Id, AnswerText = "Save it immediately.", IsCorrect = true, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { QuizQuestionId = questions[3].Id, AnswerText = "Pretend not to like it.", DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { QuizQuestionId = questions[3].Id, AnswerText = "Delete it for dramatic effect.", DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },

            new() { QuizQuestionId = questions[4].Id, AnswerText = "Enough to make them feel chosen.", IsCorrect = true, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new() { QuizQuestionId = questions[4].Id, AnswerText = "Whatever is left after everything else.", DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new() { QuizQuestionId = questions[4].Id, AnswerText = "One recycled message forever.", DisplayOrder = 3, CreatedAt = now, UpdatedAt = now }
        };

        _context.QuizAnswers.AddRange(answers);
        await _context.SaveChangesAsync();
    }

    private sealed record DefaultResultRange(int MinScore, int MaxScore, string Title, string Message, int DisplayOrder);
}
