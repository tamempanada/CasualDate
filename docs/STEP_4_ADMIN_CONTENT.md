# Step 4: Admin Content Management

This step adds the first useful admin editor pages.

## New Admin Pages

```txt
/admin/settings
/admin/media
/admin/messages
/admin/games
```

## What Each Page Does

### Settings

Edit:

- Game title
- Girlfriend's name
- Your name
- Intro message
- Final title
- Final message
- Main colors

The intro message supports:

```txt
{name}
```

Example:

```txt
Would you like to go on a date with me, {name}?
```

If the girlfriend name is `Ali`, the player sees:

```txt
Would you like to go on a date with me, Ali?
```

### Media

Upload:

- Images: `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`
- Audio: `.mp3`, `.wav`, `.ogg`, `.m4a`

Files are stored in:

```txt
wwwroot/uploads/images
wwwroot/uploads/audio
```

MySQL stores metadata only.

Current safety rules:

- Maximum file size: 8 MB
- Maximum image dimensions: 5000 x 5000
- Server-generated file names
- Unsupported extensions are rejected

### Messages

Create/edit/delete reusable messages for:

- Intro copy
- Result screens
- Unlock messages
- Final notes

These messages are not fully connected to every player screen yet. The table and editor are ready so mini-games can use them in later steps.

### Games

Edit the main menu mini-games:

- Name
- Description
- Icon label
- Display order
- Enabled/disabled state

## Migration Note

This step uses tables that already existed in the Step 2 schema.

If your database was created before Step 2 finished, run:

```powershell
dotnet ef migrations add AdminContentEditors
dotnet ef database update
```

If your database already has these tables, no new migration is required for this step.

## Next Step

Step 5 is the Player Introduction polish:

- Make the intro page fully use admin settings.
- Improve the YES/NO behavior.
- Add optional admin controls for the NO-click behavior.
- Make sure the intro looks good on mobile.
