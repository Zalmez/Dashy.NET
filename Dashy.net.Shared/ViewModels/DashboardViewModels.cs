using System.ComponentModel.DataAnnotations;

namespace Dashy.Net.Shared.ViewModels;
public record DashboardConfigVm(
    string Title,
    List<SectionVm> Sections
);

public record SectionVm(
    int Id,
    string Name,
    string? Icon,
    List<ItemVm> Items
);

public record ItemVm(
    int Id,
    [Required] string Title,
    string? Description,
    string? Url,
    string? Icon,
    string? Widget,
    int SectionId,
    Dictionary<string, object>? Options
);

