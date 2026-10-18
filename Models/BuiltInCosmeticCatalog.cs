namespace CasualDate.Models;

public enum CosmeticCategory
{
    Theme,
    MusicSkin,
    Effect
}

public sealed record BuiltInCosmetic(
    string Id,
    string Name,
    CosmeticCategory Category,
    string Description,
    string StyleKey);

public static class BuiltInCosmeticCatalog
{
    public const string DefaultThemeId = "default-theme";
    public const string DefaultMusicSkinId = "default-music";
    public const string NoEffectId = "none-effect";

    public static readonly IReadOnlyList<BuiltInCosmetic> Items =
    [
        new("theme-strawberry-milk", "Strawberry Milk", CosmeticCategory.Theme, "Soft strawberry pink and creamy white for a cozy romantic glow.", "strawberry-milk"),
        new("theme-matcha-cafe", "Matcha Cafe", CosmeticCategory.Theme, "Muted matcha green and warm cream with a quiet cafe feel.", "matcha-cafe"),
        new("theme-midnight-romance", "Midnight Romance", CosmeticCategory.Theme, "Deep plum and starlight for an elegant late-night mood.", "midnight-romance"),
        new("theme-sunset-love", "Sunset Love", CosmeticCategory.Theme, "Peach, coral, and golden-hour warmth.", "sunset-love"),
        new("theme-cloud-nine", "Cloud Nine", CosmeticCategory.Theme, "Airy lavender and pastel blue with dreamy highlights.", "cloud-nine"),
        new("music-strawberry-dreams", "Strawberry Dreams", CosmeticCategory.MusicSkin, "A soft romantic pink skin for the music player.", "strawberry-dreams"),
        new("music-matcha-moments", "Matcha Moments", CosmeticCategory.MusicSkin, "A calm cafe skin for the music player.", "matcha-moments"),
        new("music-midnight-serenade", "Midnight Serenade", CosmeticCategory.MusicSkin, "An elegant dark skin for the music player.", "midnight-serenade"),
        new("music-golden-hour", "Golden Hour", CosmeticCategory.MusicSkin, "A warm sunset-inspired music player skin.", "golden-hour"),
        new("music-cloud-nine-radio", "Cloud Nine Radio", CosmeticCategory.MusicSkin, "A light, dreamy skin for the music player.", "cloud-nine-radio"),
        new("effect-rose-petals", "Rose Petals", CosmeticCategory.Effect, "Delicate petals drift behind your pointer.", "rose-petals"),
        new("effect-stardust", "Stardust", CosmeticCategory.Effect, "Tiny glowing particles follow your pointer.", "stardust"),
        new("effect-heartbeats", "Heartbeats", CosmeticCategory.Effect, "Small hearts appear when you click.", "heartbeats"),
        new("effect-dream-sparkles", "Dream Sparkles", CosmeticCategory.Effect, "Subtle sparkles greet supported interactions.", "dream-sparkles"),
        new("effect-golden-touch", "Golden Touch", CosmeticCategory.Effect, "A restrained golden burst for your interactions.", "golden-touch")
    ];

    public static BuiltInCosmetic? Find(string? id) => Items.FirstOrDefault(item => item.Id == id);

    public static bool IsValidForCategory(string? id, CosmeticCategory category) =>
        Items.Any(item => item.Id == id && item.Category == category);
}
