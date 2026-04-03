namespace dashy3.Web.Models;

public class CollaboratorInfo
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public string Permission { get; set; } = "Viewer"; // "Viewer" | "Editor"
}

public class UserSearchResult
{
    public string Id { get; set; } = "";
    public string? Email { get; set; }
    public string DisplayName { get; set; } = "";
    public string? AvatarUrl { get; set; }
}
