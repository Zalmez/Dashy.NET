using System.ComponentModel.DataAnnotations;

namespace dashy3.ApiService.Entities;

public class ApiKeyEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Required]
    public string KeyHash { get; set; } = "";   // SHA-256 hex of raw key

    [Required, MaxLength(16)]
    public string KeyPrefix { get; set; } = ""; // first ~11 chars for display

    [Required]
    public string OwnerId { get; set; } = "";

    public bool IsGlobal { get; set; } = false;  // Admin-only: bypass all ACL

    public string? AllowedDashboardIds { get; set; } // JSON array; null = owner ACL applies

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public bool IsRevoked { get; set; } = false;

    public AppUser Owner { get; set; } = default!;
}
