using Microsoft.AspNetCore.Identity;

namespace VerdictApp.Data;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePhotoPath { get; set; }
}