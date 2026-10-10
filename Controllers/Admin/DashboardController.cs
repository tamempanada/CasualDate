using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CasualDate.Controllers.Admin;

public class DashboardController : Controller
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public IActionResult Index()
    {
        return View("~/Views/Admin/Dashboard/Index.cshtml");
    }
}
