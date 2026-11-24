using Microsoft.AspNetCore.Identity;

namespace KhoyaLedger.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
