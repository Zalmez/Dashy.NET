using System.Text.Json;

namespace dashy3.ApiService.Dtos;

// ── Response DTOs (match Web Models exactly) ──────────────────────────────────

public record DashboardDto(
    string Id,
    string Name,
    string LayoutMode,
    List<WidgetDto> Widgets,
    string? OwnerId,
    string Visibility,
    string? UserPermission); // "Owner" | "Editor" | "Viewer" | null (public view)

public record WidgetDto(
    string Id,
    string Type,
    string Title,
    string? Icon,
    string Size,
    Dictionary<string, JsonElement> Config,
    string? ParentWidgetId,
    int Order);

public record CollaboratorDto(
    string UserId,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string Permission);

// ── Request DTOs ──────────────────────────────────────────────────────────────

public record CreateDashboardRequest(string Name, string LayoutMode = "Auto", string? OwnerId = null, string Visibility = "Private");

public record UpdateDashboardVisibilityRequest(string Visibility);

public record UpdateDashboardRequest(string Name, string LayoutMode);

public record CreateWidgetRequest(
    string Type,
    string Title,
    string? Icon,
    string Size,
    Dictionary<string, JsonElement>? Config,
    string? ParentWidgetId);

public record UpdateWidgetRequest(
    string Title,
    string? Icon,
    string Size,
    Dictionary<string, JsonElement>? Config,
    string? ParentWidgetId,
    int Order);

public record AddCollaboratorRequest(string UserId, string Permission);

public record UpdateCollaboratorRequest(string Permission);

