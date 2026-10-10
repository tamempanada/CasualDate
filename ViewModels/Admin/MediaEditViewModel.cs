using System.ComponentModel.DataAnnotations;

namespace CasualDate.ViewModels.Admin;

public class MediaEditViewModel
{
    public int Id { get; set; }

    public int? MiniGameId { get; set; }

    public int? AlbumCoverMediaId { get; set; }

    [MaxLength(120)]
    public string DisplayName { get; set; } = "";

    [MaxLength(80)]
    public string Category { get; set; } = "";
}
