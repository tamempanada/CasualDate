using CasualDate.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CasualDate.ViewModels.Admin;

public class GachaAdminIndexViewModel
{
    public IReadOnlyList<GachaReward> Rewards { get; set; } = [];

    public int? GachaBannerMediaId { get; set; }

    public string? GachaBannerPath { get; set; }

    public string? GachaBannerName { get; set; }

    public List<SelectListItem> ImageMediaItems { get; set; } = [];
}
