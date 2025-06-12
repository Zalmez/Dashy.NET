using Dashy.Net.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Dashy.Net.Shared.Models;

public record CreateItemDto(
    [Required] string Title,
    string? Description,
    string? Url,
    string? Icon,
    [Required] string Widget,
    [Required] int SectionId,
    Dictionary<string, object>? Options
);

public record UpdateItemDto(
    [Required] string Title,
    string? Description,
    string? Url,
    string? Icon,
    [Required] string Widget,
    [Required] int SectionId,
    Dictionary<string, object>? Options
);

public record ReorderItemsDto(List<int> OrderedItemIds);

public record CreateSectionDto([Required] string Name, string? Icon, int DashboardId);
public record UpdateSectionDto([Required] string Name, string? Icon);
public record ReorderSectionsDto(List<int> OrderedSectionIds);

public record CreateHeaderButtonDto(
    [Required] string Text,
    string? Url,
    string? Icon,
    [Required] int DashboardId
);

public record UpdateHeaderButtonDto(
    [Required] string Text,
    string? Url,
    string? Icon
);

public record ReorderHeaderButtonsDto(List<int> OrderedButtonIds);