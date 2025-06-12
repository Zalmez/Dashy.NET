using System.ComponentModel.DataAnnotations;

namespace Dashy.Net.Shared.ViewModels;
public record DashboardConfigVm(int Id, string Title, string? Subtitle, List<SectionVm> Sections, List<HeaderButtonVm> HeaderButtons);
public record HeaderButtonVm(int Id, string Text, string? Url, string? Icon);

public record SectionVm(int Id, string Name, string? Icon, int DashboardId, List<ItemVm> Items);

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

