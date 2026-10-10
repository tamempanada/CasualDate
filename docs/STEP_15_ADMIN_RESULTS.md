# Step 15: Admin Results Editor

## What Was Added

- Protected admin page at `/admin/results`.
- Create, edit, and delete score result ranges.
- Assign each range to a mini-game.
- Edit minimum score, maximum score, title, message, and display order.
- Dashboard link to the Results editor.
- Quiz now reads custom result ranges when they exist.

## Files Added

```txt
Controllers/Admin/ResultsController.cs
ViewModels/Admin/ResultRangeEditViewModel.cs
Views/Admin/Results/Index.cshtml
Views/Admin/Results/Edit.cshtml
docs/STEP_15_ADMIN_RESULTS.md
```

## Files Updated

```txt
Controllers/GameController.cs
ViewModels/Player/QuizGameViewModel.cs
Views/Game/Quiz.cshtml
Views/Admin/Dashboard/Index.cshtml
wwwroot/js/quiz.js
docs/NEXT_STEPS.md
```

## Notes

This patch does not require a migration. It uses the existing table:

```txt
ResultRanges
```

Perfect Date already used result ranges. Quiz now uses them too. If Quiz has no matching custom result range, it falls back to its built-in result messages.

## Recommended Later Improvements

- Add result range support to Bake a Cake.
- Add image or audio attachments to result ranges.
- Add validation to detect overlapping ranges for the same mini-game.
- Add preview links from the result editor.
