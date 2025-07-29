namespace Dashy.Net.Shared.Models;

public class EditLockInfo
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime LockedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
}