namespace dashy3.ApiService.Entities;

public class DashboardCollaboratorEntity
{
    public string DashboardId { get; set; } = "";
    public DashboardEntity Dashboard { get; set; } = null!;

    public string UserId { get; set; } = "";
    public AppUser User { get; set; } = null!;

    /// <summary>"Viewer" or "Editor"</summary>
    public string Permission { get; set; } = "Viewer";

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
