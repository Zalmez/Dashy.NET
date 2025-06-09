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

public record CreateSectionDto([Required] string Name);