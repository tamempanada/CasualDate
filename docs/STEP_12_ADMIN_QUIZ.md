# Step 12: Admin Quiz Editor

## What Was Added

- Protected admin quiz page at `/admin/quiz`.
- Create, edit, and delete quiz questions.
- Edit question text, type, difficulty, points, display order, and enabled status.
- Edit up to four answers on the same form.
- Choose the correct answer with a radio button.
- Dashboard link to the quiz editor.

## Files Added

```txt
Controllers/Admin/QuizController.cs
ViewModels/Admin/QuizAnswerEditViewModel.cs
ViewModels/Admin/QuizQuestionEditViewModel.cs
Views/Admin/Quiz/Index.cshtml
Views/Admin/Quiz/Edit.cshtml
docs/STEP_12_ADMIN_QUIZ.md
```

## Files Updated

```txt
Views/Admin/Dashboard/Index.cshtml
wwwroot/css/site.css
docs/NEXT_STEPS.md
```

## Notes

This patch does not require a migration. It uses the existing quiz tables:

```txt
QuizQuestions
QuizAnswers
```

The player quiz page will automatically use the edited admin questions.

## Recommended Later Improvements

- Add image uploads per question or answer.
- Add more than four answers with dynamic rows.
- Add preview mode from the admin editor.
- Add result ranges editable from the admin dashboard.
