# Step 10: Quiz

## What Was Added

- A playable quiz at `/Game/Quiz`.
- Starter questions and answers inserted automatically if the quiz table is empty.
- Multiple choice and true/false support through the existing quiz schema.
- Live score and progress tracking.
- Result messages based on percentage score.
- Completion support through the existing localStorage progress system.

## Files Added

```txt
ViewModels/Player/QuizAnswerChoiceViewModel.cs
ViewModels/Player/QuizQuestionViewModel.cs
ViewModels/Player/QuizGameViewModel.cs
wwwroot/js/quiz.js
docs/STEP_10_QUIZ.md
```

## Files Updated

```txt
Controllers/GameController.cs
Views/Game/Quiz.cshtml
wwwroot/css/site.css
docs/NEXT_STEPS.md
```

## Notes

This patch does not require a new migration. It uses existing tables:

```txt
QuizQuestions
QuizAnswers
```

The current questions are starter content. Later, the admin dashboard should get a quiz editor so the questions can become personal and editable without code changes.

## Recommended Later Improvements

- Admin CRUD for quiz questions and answers.
- Image-based quiz questions.
- Per-question feedback after answering.
- Custom result ranges controlled from the admin dashboard.
- Optional question categories for friend/couple versions.
