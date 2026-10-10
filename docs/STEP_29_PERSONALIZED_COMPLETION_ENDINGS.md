# Step 29: Personalized Completion Endings

This patch makes each mini-game finish with a more personal completion/result moment while keeping the existing gameplay intact.

## Photobooth

- The player now sees the actual generated photo strip in a centered completion panel.
- The completion heading and subheading are editable in `Admin > Settings`.
- The progress-saving button appears inside the completion panel, so the player sees the photo before returning to the menu.

## Bake a Cake

- The cake result now uses editable score ranges from `Admin > Results`.
- The completed cake is shown again in the result panel.
- The result panel shows the final grade and the editable grade message.
- Optional result images can be selected from uploaded media.

## Perfect Date

- Perfect Date still calculates the final score from the selected date options.
- It then finds all result entries whose score range contains that score.
- If multiple matching endings exist, one is randomly selected.
- This lets the admin create multiple Bad, Normal, or Good endings by giving entries in the same category the same score range.

Example:

```txt
Bad Ending 1:    0 - 79
Bad Ending 2:    0 - 79
Normal Ending 1: 80 - 91
Good Ending 1:   92 - 120
Good Ending 2:   92 - 120
```

A low score only matches the Bad range, so it cannot receive Normal or Good endings.

## Quiz

- Quiz still calculates the final score from correct answers.
- It displays the single matching result range.
- No randomization is used unless the admin intentionally creates duplicate matching ranges, so keep exactly one Bad, one Normal, and one Good entry for Quiz.
- Optional result images can be selected from uploaded media.

## Admin Usage

- Use `Admin > Settings` for Photobooth completion heading/subheading.
- Use `Admin > Media` to upload ending images.
- Use `Admin > Results` to manage Cake, Perfect Date, and Quiz endings.
- Use the Result image field to attach uploaded images to result screens.

## Database

This step adds two fields to `GameSettings`:

```txt
PhotoboothCompletionHeading
PhotoboothCompletionSubheading
```

Run:

```powershell
dotnet ef database update
```

## Verification

- `node --check wwwroot/js/photobooth.js`
- `node --check wwwroot/js/cake.js`
- `node --check wwwroot/js/perfect-date.js`
- `node --check wwwroot/js/quiz.js`
- `dotnet build --no-restore -o ..\work\build-check`

The build still shows the existing ImageSharp license warning, but there are no compile errors.
