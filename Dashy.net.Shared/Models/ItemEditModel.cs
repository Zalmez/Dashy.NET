using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashy.Net.Shared.Models;
public class ItemEditModel
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = "";

    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public string Widget { get; set; } = "static-link";
    public int SectionId { get; set; }
    public Dictionary<string, object>? Options { get; set; }
}
