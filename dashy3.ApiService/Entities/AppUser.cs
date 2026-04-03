using Microsoft.AspNetCore.Identity;

namespace dashy3.ApiService.Entities;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public string? OidcSub { get; set; }   // Stable OIDC subject identifier
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
