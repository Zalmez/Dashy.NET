namespace dashy3.ApiService.Entities;

public class InviteEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Token { get; set; } = Guid.NewGuid().ToString("N"); // Secret token for invite URL
    public string Email { get; set; } = "";
    public string Role { get; set; } = "User"; // User or Admin
    public string? InvitedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public string? AcceptedByUserId { get; set; } // Filled when invite is accepted
}
