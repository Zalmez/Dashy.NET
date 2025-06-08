using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashy.Net.Shared.DTOs;

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

