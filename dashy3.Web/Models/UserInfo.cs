namespace dashy3.Web.Models;

public class UserInfo
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public string[] Roles { get; set; } = [];
    public DateTime CreatedAt { get; set; }

    public bool IsAdmin => Roles.Contains("Admin");
    public DateTime? LastLoginAt { get; set; }
    public string Initials => string.IsNullOrEmpty(DisplayName)
        ? (string.IsNullOrEmpty(Email) ? "?" : Email[0].ToString().ToUpper())
        : string.Concat(DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(w => char.ToUpper(w[0])));
}
