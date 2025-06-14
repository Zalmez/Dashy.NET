using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
    public Dictionary<string, object> Options { get; set; } = new();
}
public class UpdateItemDto
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Widget { get; set; }
    public int SectionId { get; set; }
    public Dictionary<string, object> Options { get; set; } = new();
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


// --- ViewModels (Models for the Blazor UI) ---
public record DashboardConfigVm(int Id, string Title, string? Subtitle, List<SectionVm> Sections, List<HeaderButtonVm> HeaderButtons);
public record SectionVm(int Id, string Name, string? Icon, int DashboardId, List<ItemVm> Items);
public record HeaderButtonVm(int Id, string Text, string? Url, string? Icon);
public record ItemVm(
    int Id,
    string Title,
    string? Icon,
    string? Widget,
    int SectionId,
    Dictionary<string, object>? Options
);
