using Microsoft.AspNetCore.Identity;

namespace CasualDate.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
