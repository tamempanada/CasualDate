using System.ComponentModel.DataAnnotations;

namespace CasualDate.Models;

public class UploadedMedia
{
    public int Id { get; set; }

    public int? MiniGameId { get; set; }

    public int? AlbumCoverMediaId { get; set; }

    [MaxLength(255)]
    public string OriginalFileName { get; set; } = "";

    [MaxLength(255)]
    public string StoredFileName { get; set; } = "";

    [MaxLength(500)]
    public string FilePath { get; set; } = "";

    [MaxLength(120)]
    public string ContentType { get; set; } = "";

    public MediaType MediaType { get; set; }

    [MaxLength(80)]
    public string Category { get; set; } = "";

    [MaxLength(120)]
    public string DisplayName { get; set; } = "";

    public long FileSizeBytes { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
