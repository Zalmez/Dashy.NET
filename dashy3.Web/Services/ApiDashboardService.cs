using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using dashy3.Web.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace dashy3.Web.Services;

/// <summary>
/// API-backed dashboard service. Maintains a local in-memory cache for
/// instant UI responsiveness; mutations are persisted via the API.
/// </summary>
public class ApiDashboardService(
    IHttpClientFactory httpClientFactory,
    IToastService toast,
    AuthenticationStateProvider authStateProvider,
    ILogger<ApiDashboardService> logger) : IDashboardService
{
    private readonly HttpClient http = httpClientFactory.CreateClient("dashboards-api");
    private readonly List<Dashboard> _dashboards = [];
    private string _activeDashboardId = "";
    private bool _initialized;

    private async Task<HttpRequestMessage> AddUserIdAsync(HttpRequestMessage request)
    {
        try
        {
            var authState = await authStateProvider.GetAuthenticationStateAsync();
            var userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
                request.Headers.TryAddWithoutValidation("X-User-Id", userId);
        }
        catch { /* non-critical — request proceeds without the header */ }
        return request;
    }

    public event Action? StateChanged;

    public IReadOnlyList<Dashboard> Dashboards => _dashboards.AsReadOnly();
    public string ActiveDashboardId => _activeDashboardId;
    public Dashboard? ActiveDashboard => _dashboards.FirstOrDefault(d => d.Id == _activeDashboardId);

        public async Task InitializeAsync()
    {
        if (_initialized) return;

        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Get, "/api/dashboards"));
            var response = await http.SendAsync(req);
            var dashboards = await response.Content.ReadFromJsonAsync<List<Dashboard>>(JsonOptions);
            if (dashboards is not null)
            {
                _dashboards.Clear();
                _dashboards.AddRange(dashboards);
            }
            _activeDashboardId = _dashboards.FirstOrDefault()?.Id ?? "";
            _initialized = true; // only mark initialized on success
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load dashboards");
            toast.ShowError($"Failed to load dashboards: {ex.Message}");
        }

        NotifyStateChanged();
    }

    public void SetActiveDashboard(string id)
    {
        _activeDashboardId = id;
        NotifyStateChanged();
    }

    public async Task<Dashboard> CreateDashboardAsync(string name)
    {
        var optimistic = new Dashboard { Name = name };
        _dashboards.Add(optimistic);
        _activeDashboardId = optimistic.Id;
        NotifyStateChanged();

        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Post, "/api/dashboards"));
            req.Content = JsonContent.Create(new { name, layoutMode = "Auto" });
            var created = await http.SendAsync(req);
            created.EnsureSuccessStatusCode();
            var dto = await created.Content.ReadFromJsonAsync<Dashboard>(JsonOptions);
            if (dto is not null)
            {
                // Replace optimistic placeholder with server-assigned id
                _dashboards.Remove(optimistic);
                _dashboards.Add(dto);
                _activeDashboardId = dto.Id;
                NotifyStateChanged();
                return dto;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create dashboard");
            toast.ShowError($"Failed to create dashboard: {ex.Message}");
            _dashboards.Remove(optimistic);
            _activeDashboardId = _dashboards.FirstOrDefault()?.Id ?? "";
            NotifyStateChanged();
        }

        return optimistic;
    }

    public async Task RenameDashboardAsync(string id, string name)
    {
        var dashboard = _dashboards.FirstOrDefault(d => d.Id == id);
        if (dashboard is null) return;

        var old = dashboard.Name;
        dashboard.Name = name;
        NotifyStateChanged();

        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Put, $"/api/dashboards/{id}"));
            req.Content = JsonContent.Create(new { 
                name, 
                layoutMode = dashboard.LayoutMode.ToString(),
                autoScroll = dashboard.AutoScroll,
                scrollSpeed = dashboard.ScrollSpeed
            });
            var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            dashboard.Name = old;
            logger.LogError(ex, "Failed to rename dashboard {DashboardId}", id);
            toast.ShowError($"Failed to rename dashboard: {ex.Message}");
            NotifyStateChanged();
        }
    }

    public async Task SetDashboardLayoutAsync(string id, LayoutMode mode)
    {
        var dashboard = _dashboards.FirstOrDefault(d => d.Id == id);
        if (dashboard is null) return;

        var old = dashboard.LayoutMode;
        dashboard.LayoutMode = mode;
        NotifyStateChanged();

        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Put, $"/api/dashboards/{id}"));
            req.Content = JsonContent.Create(new { 
                name = dashboard.Name, 
                layoutMode = mode.ToString(),
                autoScroll = dashboard.AutoScroll,
                scrollSpeed = dashboard.ScrollSpeed
            });
            var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            dashboard.LayoutMode = old;
            logger.LogError(ex, "Failed to update layout for dashboard {DashboardId}", id);
            toast.ShowError($"Failed to update layout: {ex.Message}");
            NotifyStateChanged();
        }
    }

    public async Task<bool> UpdateDashboardVisibilityAsync(string id, string visibility)
    {
        var dashboard = _dashboards.FirstOrDefault(d => d.Id == id);
        if (dashboard is null) return false;

        var old = dashboard.Visibility;
        dashboard.Visibility = visibility;
        NotifyStateChanged();

        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/dashboards/{id}/visibility"));
            req.Content = JsonContent.Create(new { visibility });
            var res = await http.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                dashboard.Visibility = old;
                toast.ShowError("Failed to update visibility");
                NotifyStateChanged();
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            dashboard.Visibility = old;
            logger.LogError(ex, "Failed to update visibility for dashboard {DashboardId}", id);
            toast.ShowError($"Failed to update visibility: {ex.Message}");
            NotifyStateChanged();
            return false;
        }
    }

    public async Task UpdateDashboardAutoScrollAsync(string id, bool autoScroll, string scrollSpeed)
    {
        var dashboard = _dashboards.FirstOrDefault(d => d.Id == id);
        if (dashboard is null) return;

        var oldAutoScroll = dashboard.AutoScroll;
        var oldScrollSpeed = dashboard.ScrollSpeed;
        dashboard.AutoScroll = autoScroll;
        dashboard.ScrollSpeed = scrollSpeed;
        NotifyStateChanged();

        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Put, $"/api/dashboards/{id}"));
            req.Content = JsonContent.Create(new { 
                name = dashboard.Name, 
                layoutMode = dashboard.LayoutMode.ToString(),
                autoScroll,
                scrollSpeed
            });
            var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            dashboard.AutoScroll = oldAutoScroll;
            dashboard.ScrollSpeed = oldScrollSpeed;
            logger.LogError(ex, "Failed to update auto-scroll for dashboard {DashboardId}", id);
            toast.ShowError($"Failed to update auto-scroll: {ex.Message}");
            NotifyStateChanged();
        }
    }

    public async Task DeleteDashboardAsync(string id)
    {
        var dashboard = _dashboards.FirstOrDefault(d => d.Id == id);
        if (dashboard is null) return;

        _dashboards.Remove(dashboard);
        if (_activeDashboardId == id)
            _activeDashboardId = _dashboards.FirstOrDefault()?.Id ?? "";
        NotifyStateChanged();

        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/dashboards/{id}"));
            var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _dashboards.Add(dashboard);
            logger.LogError(ex, "Failed to delete dashboard {DashboardId}", id);
            toast.ShowError($"Failed to delete dashboard: {ex.Message}");
            NotifyStateChanged();
        }
    }

    public async Task AddWidgetAsync(Widget widget)
    {
        var dashboard = ActiveDashboard;
        if (dashboard is null) return;

        widget.Order = dashboard.Widgets.Count;
        dashboard.Widgets.Add(widget);
        NotifyStateChanged();

        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Post, $"/api/dashboards/{dashboard.Id}/widgets"));
            req.Content = JsonContent.Create(new
            {
                type = widget.Type,
                title = widget.Title,
                icon = widget.Icon,
                size = widget.Size.ToString(),
                config = widget.Config.ToDictionary(
                    kvp => kvp.Key,
                    kvp => JsonSerializer.SerializeToElement(kvp.Value)),
                parentWidgetId = widget.ParentWidgetId
            });
            var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var created = await res.Content.ReadFromJsonAsync<Widget>(JsonOptions);
            if (created is not null)
            {
                dashboard.Widgets.Remove(widget);
                dashboard.Widgets.Add(created);
                NotifyStateChanged();
            }
        }
        catch (Exception ex)
        {
            dashboard.Widgets.Remove(widget);
            logger.LogError(ex, "Failed to add widget to dashboard {DashboardId}", dashboard.Id);
            toast.ShowError($"Failed to add widget: {ex.Message}");
            NotifyStateChanged();
        }
    }

    public async Task SaveWidgetAsync(Widget widget)
    {
        var dashboard = _dashboards.FirstOrDefault(d => d.Widgets.Any(w => w.Id == widget.Id));
        if (dashboard is null) return;

        var existing = dashboard.Widgets.FirstOrDefault(w => w.Id == widget.Id);
        if (existing is null) return;

                existing.Title = widget.Title;
        existing.Icon = widget.Icon;
        existing.Size = widget.Size;
        existing.ParentWidgetId = widget.ParentWidgetId;
        existing.Order = widget.Order;
        existing.Config = widget.Config;
        NotifyStateChanged();

        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Put, $"/api/dashboards/{dashboard.Id}/widgets/{widget.Id}"));
            req.Content = JsonContent.Create(new
            {
                title = widget.Title,
                icon = widget.Icon,
                size = widget.Size.ToString(),
                config = widget.Config.ToDictionary(
                    kvp => kvp.Key,
                    kvp => JsonSerializer.SerializeToElement(kvp.Value)),
                parentWidgetId = widget.ParentWidgetId,
                order = widget.Order
            });
            var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save widget {WidgetId}", widget.Id);
            toast.ShowError($"Failed to save widget: {ex.Message}");
        }
    }

        public async Task ReorderWidgetsAsync(string draggedId, string targetId, bool insertBefore = false)
        {
            if (draggedId == targetId) return;
            var dashboard = _dashboards.FirstOrDefault(d => d.Widgets.Any(w => w.Id == draggedId));
            if (dashboard is null) return;

            var dragged = dashboard.Widgets.FirstOrDefault(w => w.Id == draggedId);
            var target = dashboard.Widgets.FirstOrDefault(w => w.Id == targetId);
            if (dragged is null || target is null) return;

                        var newParentId = target.ParentWidgetId;

                        if (dragged.Type == WidgetType.Section)
                        {
                            if (newParentId == draggedId)
                {
                    toast.ShowError("Cannot move a section into itself");
                    return;
                }

                var currentParent = newParentId;
                while (currentParent is not null)
                {
                    if (currentParent == draggedId)
                    {
                        toast.ShowError("Cannot move a section into its own child");
                        return;
                    }
                    var parentWidget = dashboard.Widgets.FirstOrDefault(w => w.Id == currentParent);
                    currentParent = parentWidget?.ParentWidgetId;
                }
            }

            var oldParentId = dragged.ParentWidgetId;
            var oldOrder = dragged.Order;

                        dragged.ParentWidgetId = newParentId;

                        var siblings = dashboard.Widgets
                .Where(w => w.ParentWidgetId == newParentId && w.Id != draggedId)
                .OrderBy(w => w.Order)
                .ToList();

                            var targetIndex = siblings.FindIndex(w => w.Id == targetId);
            if (targetIndex < 0) targetIndex = 0;
            var newIndex = insertBefore ? targetIndex : targetIndex + 1;

                        newIndex = Math.Max(0, Math.Min(newIndex, siblings.Count));

                        siblings.Insert(newIndex, dragged);

                        for (int i = 0; i < siblings.Count; i++)
            {
                siblings[i].Order = i;
            }

            NotifyStateChanged();

            try
            {
                // Save all affected widgets
                var saveTaskList = siblings.Select(SaveWidgetAsync).ToList();
                await Task.WhenAll(saveTaskList);
            }
            catch (Exception ex)
            {
                // Rollback on failure
                dragged.ParentWidgetId = oldParentId;
                dragged.Order = oldOrder;
                logger.LogError(ex, "Failed to reorder widgets in dashboard {DashboardId}", dashboard.Id);
                toast.ShowError($"Failed to reorder: {ex.Message}");
                NotifyStateChanged();
            }
        }

        public async Task DeleteWidgetAsync(string widgetId)
    {
        var dashboard = _dashboards.FirstOrDefault(d => d.Widgets.Any(w => w.Id == widgetId));
        var widget = dashboard?.Widgets.FirstOrDefault(w => w.Id == widgetId);
        if (dashboard is null || widget is null) return;

        dashboard.Widgets.Remove(widget);
        NotifyStateChanged();

        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/dashboards/{dashboard.Id}/widgets/{widgetId}"));
            var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            dashboard.Widgets.Add(widget);
            logger.LogError(ex, "Failed to delete widget {WidgetId} from dashboard {DashboardId}", widgetId, dashboard.Id);
            toast.ShowError($"Failed to delete widget: {ex.Message}");
            NotifyStateChanged();
        }
    }

    public async Task MoveWidgetToSectionAsync(string widgetId, string? newParentWidgetId)
    {
        var dashboard = _dashboards.FirstOrDefault(d => d.Widgets.Any(w => w.Id == widgetId));
        var widget = dashboard?.Widgets.FirstOrDefault(w => w.Id == widgetId);
        if (dashboard is null || widget is null) return;

                if (widgetId == newParentWidgetId) return;

                if (newParentWidgetId is not null)
        {
            var parentWidget = dashboard.Widgets.FirstOrDefault(w => w.Id == newParentWidgetId);
            if (parentWidget is null || parentWidget.Type != WidgetType.Section) return;

                        if (widget.Type == WidgetType.Section && parentWidget.ParentWidgetId is not null) return;
        }

        var oldParentId = widget.ParentWidgetId;
        widget.ParentWidgetId = newParentWidgetId;

                var siblings = newParentWidgetId is null
            ? dashboard.Widgets.Where(w => w.ParentWidgetId is null && w.Id != widgetId)
            : dashboard.Widgets.Where(w => w.ParentWidgetId == newParentWidgetId && w.Id != widgetId);
        widget.Order = siblings.Any() ? siblings.Max(w => w.Order) + 1 : 0;

        NotifyStateChanged();

        try
        {
            await SaveWidgetAsync(widget);
        }
        catch (Exception ex)
        {
            widget.ParentWidgetId = oldParentId;
            logger.LogError(ex, "Failed to move widget {WidgetId} to section", widgetId);
            NotifyStateChanged();
        }
    }

    public async Task<List<CollaboratorInfo>> GetCollaboratorsAsync(string dashboardId)
    {
        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Get, $"/api/dashboards/{dashboardId}/collaborators"));
            var res = await http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return [];
            return await res.Content.ReadFromJsonAsync<List<CollaboratorInfo>>(JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get collaborators for dashboard {DashboardId}", dashboardId);
            return [];
        }
    }

    public async Task<bool> AddCollaboratorAsync(string dashboardId, string userId, string permission)
    {
        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Post, $"/api/dashboards/{dashboardId}/collaborators"));
            req.Content = JsonContent.Create(new { userId, permission });
            var res = await http.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                var err = await TryReadErrorAsync(res);
                toast.ShowError(err ?? "Failed to add collaborator");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add collaborator {UserId} to dashboard {DashboardId}", userId, dashboardId);
            toast.ShowError($"Failed to add collaborator: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateCollaboratorAsync(string dashboardId, string userId, string permission)
    {
        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Put, $"/api/dashboards/{dashboardId}/collaborators/{userId}"));
            req.Content = JsonContent.Create(new { permission });
            var res = await http.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                toast.ShowError("Failed to update collaborator permission");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update collaborator {UserId} on dashboard {DashboardId}", userId, dashboardId);
            toast.ShowError($"Failed to update collaborator: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveCollaboratorAsync(string dashboardId, string userId)
    {
        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/dashboards/{dashboardId}/collaborators/{userId}"));
            var res = await http.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                toast.ShowError("Failed to remove collaborator");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove collaborator {UserId} from dashboard {DashboardId}", userId, dashboardId);
            toast.ShowError($"Failed to remove collaborator: {ex.Message}");
            return false;
        }
    }

    public async Task<List<UserSearchResult>> SearchUsersAsync(string query)
    {
        try
        {
            var req = await AddUserIdAsync(new HttpRequestMessage(HttpMethod.Get, $"/api/users/search?q={Uri.EscapeDataString(query)}"));
            var res = await http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return [];
            return await res.Content.ReadFromJsonAsync<List<UserSearchResult>>(JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to search users with query {Query}", query);
            return [];
        }
    }

    private static async Task<string?> TryReadErrorAsync(HttpResponseMessage res)
    {
        try
        {
            var obj = await res.Content.ReadFromJsonAsync<JsonElement>();
            if (obj.TryGetProperty("error", out var err)) return err.GetString();
        }
        catch { }
        return null;
    }

    // ── UI-only ───────────────────────────────────────────────────────────────

    public void ToggleWidgetEdit(string widgetId, string userId = "Current User")
    {
        var widget = _dashboards.SelectMany(d => d.Widgets).FirstOrDefault(w => w.Id == widgetId);
        if (widget is null) return;

        widget.IsBeingEdited = !widget.IsBeingEdited;
        widget.EditedBy = widget.IsBeingEdited ? userId : null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new SafeLayoutModeConverter(), // must precede JsonStringEnumConverter to handle unknown values
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        }
    };
}

