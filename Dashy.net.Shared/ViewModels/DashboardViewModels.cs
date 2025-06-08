using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashy.Net.Shared.ViewModels;
public record DashboardConfigVm(
    string Title,
    List<SectionVm> Sections
);

public record SectionVm(
    int Id,
    string Name,
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

