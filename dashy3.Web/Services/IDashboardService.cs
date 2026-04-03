using dashy3.Web.Models;

namespace dashy3.Web.Services;

public interface IDashboardService
{
    IReadOnlyList<Dashboard> Dashboards { get; }
    string ActiveDashboardId { get; }
    Dashboard? ActiveDashboard { get; }

    event Action? StateChanged;

    Task InitializeAsync();
    void SetActiveDashboard(string id);

    Task<Dashboard> CreateDashboardAsync(string name);
    Task RenameDashboardAsync(string id, string name);
    Task SetDashboardLayoutAsync(string id, LayoutMode mode);
    Task<bool> UpdateDashboardVisibilityAsync(string id, string visibility);
    Task DeleteDashboardAsync(string id);

    Task AddWidgetAsync(Widget widget);
    Task SaveWidgetAsync(Widget widget);
    Task DeleteWidgetAsync(string widgetId);

    /// <summary>
    /// Reorders widgets by moving the dragged widget relative to the target.
    /// </summary>
    Task ReorderWidgetsAsync(string draggedId, string targetId, bool insertBefore = false);

    /// <summary>
    /// Moves a widget into or out of a section.
    /// </summary>
    Task MoveWidgetToSectionAsync(string widgetId, string? newParentWidgetId);

    // UI-only — no API call needed
    void ToggleWidgetEdit(string widgetId, string userId = "Current User");

    // ── Collaborators ─────────────────────────────────────────────────────────

    Task<List<CollaboratorInfo>> GetCollaboratorsAsync(string dashboardId);
    Task<bool> AddCollaboratorAsync(string dashboardId, string userId, string permission);
    Task<bool> UpdateCollaboratorAsync(string dashboardId, string userId, string permission);
    Task<bool> RemoveCollaboratorAsync(string dashboardId, string userId);
    Task<List<UserSearchResult>> SearchUsersAsync(string query);
}
