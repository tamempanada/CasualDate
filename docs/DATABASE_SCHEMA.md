# Casual Date Database Schema Plan

This file describes the MySQL schema before implementation. The actual EF Core models and migrations should be added in Step 2.

## Identity Tables

ASP.NET Core Identity will create these tables:

```txt
AspNetUsers
AspNetRoles
AspNetUserRoles
AspNetUserClaims
AspNetUserLogins
AspNetUserTokens
AspNetRoleClaims
```

These store the admin account, password hash, login data, and admin role.

## Custom Tables

### GameSettings

Stores global game configuration.

```txt
Id
GameTitle
GirlfriendName
PlayerName
NicknamesJson
IntroMessage
FinalTitle
FinalMessage
PrimaryColor
SecondaryColor
BackgroundMediaId
MusicMediaId
CreatedAt
UpdatedAt
```

Recommendation: keep this as a single-row settings table at first.

### MiniGames

Stores each mini-game shown in the menu.

```txt
Id
Slug
Name
Description
DisplayOrder
IsEnabled
IconLabel
CreatedAt
UpdatedAt
```

Examples of `Slug`:

```txt
photobooth
cake
perfect-date
quiz
```

### UploadedMedia

Stores metadata for uploaded images and audio.

```txt
Id
MiniGameId nullable
OriginalFileName
StoredFileName
FilePath
ContentType
MediaType
Category
DisplayName
FileSizeBytes
Width nullable
Height nullable
CreatedAt
UpdatedAt
```

`MediaType` can be:

```txt
Image
Audio
```

### Messages

Stores editable text blocks.

```txt
Id
MiniGameId nullable
Key
Title
Body
ButtonText
MediaId nullable
DisplayOrder
CreatedAt
UpdatedAt
```

Example keys:

```txt
intro.date-request
menu.welcome
photobooth.start
final.reward
```

### ResultRanges

Stores configurable result messages for score ranges.

```txt
Id
MiniGameId
MinScore
MaxScore
Title
Message
MediaId nullable
DisplayOrder
CreatedAt
UpdatedAt
```

Used by Cake, Perfect Date, and Quiz.

### DateCategories

Stores Perfect Date categories.

```txt
Id
Name
Description
DisplayOrder
IsEnabled
CreatedAt
UpdatedAt
```

Examples:

```txt
Location
Food
Activity
Ending
```

### DateOptions

Stores options inside each Perfect Date category.

```txt
Id
DateCategoryId
Name
Description
Score
MediaId nullable
DisplayOrder
IsEnabled
CreatedAt
UpdatedAt
```

### CakeItems

Stores cake bases, frostings, toppings, and decorations.

```txt
Id
ItemType
Name
Description
Score
MediaId nullable
DisplayOrder
IsEnabled
CreatedAt
UpdatedAt
```

`ItemType` can be:

```txt
Base
Frosting
Topping
Decoration
```

### CakeRules

Stores special scoring or funny combination rules.

```txt
Id
Name
BaseItemId nullable
FrostingItemId nullable
ToppingItemId nullable
DecorationItemId nullable
BonusScore
ResultTitle
ResultMessage
IsEnabled
CreatedAt
UpdatedAt
```

Example: chocolate base plus chocolate frosting plus chocolate topping can produce a custom message.

### QuizQuestions

Stores quiz questions.

```txt
Id
QuestionText
QuestionType
Difficulty
Points
MediaId nullable
DisplayOrder
IsEnabled
CreatedAt
UpdatedAt
```

`QuestionType` can start with:

```txt
MultipleChoice
TrueFalse
ImageChoice
```

### QuizAnswers

Stores answers for each question.

```txt
Id
QuizQuestionId
AnswerText
MediaId nullable
IsCorrect
DisplayOrder
CreatedAt
UpdatedAt
```

## Relationship Summary

```txt
MiniGames 1 -> many UploadedMedia
MiniGames 1 -> many Messages
MiniGames 1 -> many ResultRanges

UploadedMedia 1 -> many optional references from settings, messages, questions, choices, and items

DateCategories 1 -> many DateOptions

CakeItems can be referenced by CakeRules

QuizQuestions 1 -> many QuizAnswers
```

## What Belongs in MySQL

- Game settings.
- Admin-customizable messages.
- Uploaded media metadata.
- Mini-game names, descriptions, order, and enabled state.
- Quiz questions and answers.
- Cake ingredients and scoring rules.
- Perfect Date categories, options, and score ranges.

## What Belongs in Browser localStorage

- Completed games.
- Player scores.
- Final reward unlock state.
- Lightweight progress data.

Do not create normal player accounts unless the project later needs multiple players or cross-device progress.
