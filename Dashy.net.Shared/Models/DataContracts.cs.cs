using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Dashy.Net.Shared.Models;

// --- DTOs (Data Transfer Objects for API communication) ---

public record CreateItemDto([Required] string Title, string? Icon, [Required] string Widget, [Required] int SectionId, Dictionary<string, object>? Options);
public record UpdateItemDto([Required] string Title, string? Icon, [Required] string Widget, [Required] int SectionId, Dictionary<string, object>? Options);
public record CreateSectionDto([Required] string Name, string? Icon, [Required] int DashboardId);
public record UpdateSectionDto([Required] string Name, string? Icon);
public record ReorderItemsDto(List<int> OrderedItemIds);
public record ReorderSectionsDto(List<int> OrderedSectionIds);
public record CreateHeaderButtonDto([Required] string Text, string? Url, string? Icon, [Required] int DashboardId);
public record UpdateHeaderButtonDto([Required] string Text, string? Url, string? Icon);
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
