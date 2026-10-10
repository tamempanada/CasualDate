using System.ComponentModel.DataAnnotations;

namespace CasualDate.ViewModels.Admin;

public class MessageEditViewModel
{
    public int Id { get; set; }

    public int? MiniGameId { get; set; }

    [Required]
    [MaxLength(120)]
    public string Key { get; set; } = "";

    [MaxLength(160)]
    public string Title { get; set; } = "";

    public string Body { get; set; } = "";

    [MaxLength(80)]
    public string ButtonText { get; set; } = "";

    public int DisplayOrder { get; set; }
}
