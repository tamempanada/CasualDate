using System.ComponentModel.DataAnnotations;

namespace CasualDate.ViewModels.Admin;

public class MediaUploadViewModel
{
    [Required]
    public IFormFile? File { get; set; }

    public int? MiniGameId { get; set; }

    [MaxLength(120)]
    public string DisplayName { get; set; } = "";

    [MaxLength(80)]
    public string Category { get; set; } = "";
}
