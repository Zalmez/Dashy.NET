using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Dashy.Net.Shared.Models;

// --- DTOs (Data Transfer Objects for API communication) ---

public class CreateItemDto
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Widget { get; set; }
    public int SectionId { get; set; }
    public int? ParentItemId { get; set; } // New: optional parent container item
    public Dictionary<string, object> Options { get; set; } = new();
    public CreateItemDto() { }
    public CreateItemDto(string title, string? icon, string? widget, int sectionId, Dictionary<string, object> options, int? parentItemId = null)
    {
        Title = title;
        Icon = icon;
        Widget = widget;
        SectionId = sectionId;
        ParentItemId = parentItemId;
        Options = options;
    }
}
public class UpdateItemDto
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Widget { get; set; }
    public int SectionId { get; set; }
    public int? ParentItemId { get; set; } // New: optional parent container item
    public Dictionary<string, object> Options { get; set; } = new();

    public UpdateItemDto() { }
    public UpdateItemDto(string title, string? icon, string? widget, int sectionId, Dictionary<string, object> options, int? parentItemId = null)
    {
        Title = title;
        Icon = icon;
        Widget = widget;
        SectionId = sectionId;
        ParentItemId = parentItemId;
        Options = options;
    }
}
public class CreateSectionDto
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DashboardId { get; set; }

    public CreateSectionDto() { }

    public CreateSectionDto(string name, string? icon, int dashboardId)
    {
        Name = name;
        Icon = icon;
        DashboardId = dashboardId;
    }
}

public class UpdateSectionDto
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }

    public UpdateSectionDto() { }

    public UpdateSectionDto(string name, string? icon)
    {
        Name = name;
        Icon = icon;
    }
}

public record ReorderItemsDto(List<int> OrderedItemIds);
public record ReorderSectionsDto(List<int> OrderedSectionIds);
public class CreateHeaderButtonDto
{
    [Required(AllowEmptyStrings = false)]
    public string Text { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DashboardId { get; set; }
}
public class UpdateHeaderButtonDto
{
    [Required(AllowEmptyStrings = false)]
    public string Text { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
}
public record ReorderHeaderButtonsDto(List<int> OrderedButtonIds);

public class CreateDashboardDto
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Subtitle { get; set; }
}

public class UpdateDashboardDto
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Subtitle { get; set; }
}

// --- Authentication Provider DTOs ---

public class CreateAuthenticationProviderDto
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    [StringLength(50)]
    public string ProviderType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = false;
    public bool IsDefault { get; set; } = false;
    public int Priority { get; set; } = 0;
    public Dictionary<string, AuthenticationProviderSettingDto> Settings { get; set; } = new();
}

public class UpdateAuthenticationProviderDto
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = false;
    public bool IsDefault { get; set; } = false;
    public int Priority { get; set; } = 0;
    public Dictionary<string, AuthenticationProviderSettingDto> Settings { get; set; } = new();
}

public class AuthenticationProviderSettingDto
{
    public string? Value { get; set; }
    public bool IsEncrypted { get; set; } = false;
    public bool IsRequired { get; set; } = false;
}

// --- ViewModels (Models for the Blazor UI) ---
public record DashboardConfigVm(int Id, string Title, string? Subtitle, List<SectionVm> Sections, List<HeaderButtonVm> HeaderButtons, bool UseContainerWidgets = false);
public record DashboardListItemVm(int Id, string Title, string? Subtitle);
[Obsolete("Sections will be replaced by container widgets; use ItemVm + section-container widget.")]
public record SectionVm(int Id, string Name, string? Icon, int DashboardId, List<ItemVm> Items);
public record HeaderButtonVm(int Id, string Text, string? Url, string? Icon);
public record ItemVm(
    int Id,
    string Title,
    string? Icon,
    string? Widget,
    int SectionId,
    JsonElement? Options,
    int? ParentItemId = null
);

public record AuthenticationProviderVm(
    int Id,
    string Name,
    string ProviderType,
    bool IsEnabled,
    bool IsDefault,
    int Priority,
    Dictionary<string, AuthenticationProviderSettingVm> Settings
);

public record AuthenticationProviderSettingVm(
    string? Value,
    bool IsEncrypted,
    bool IsRequired
);

// --- Authentication Provider Templates ---
public class AuthenticationProviderTemplate
{
    public string Name { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, AuthenticationProviderSettingTemplate> Settings { get; set; } = new();
}

public class AuthenticationProviderSettingTemplate
{
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
    public bool IsRequired { get; set; } = false;
    public bool IsEncrypted { get; set; } = false;
    public bool IsSecret { get; set; } = false;
    public string InputType { get; set; } = "text";
    public List<string>? Options { get; set; }
}
public record ReorderItemsScopedDto(int SectionId, int? ParentItemId, List<int> OrderedItemIds);
