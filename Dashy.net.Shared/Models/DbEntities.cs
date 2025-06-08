using System.ComponentModel.DataAnnotations;

namespace Dashy.Net.Shared.Models;

public class DashboardItem
{
    public int Id { get; set; }
    [Required]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Url {get; set;}
    public string? Icon { get; set; }
    public string Widget { get; set; } = "static-link";
    public string? OptionsJson { get; set; }

    public int SectionId { get; set; }
    public DashboardSection Section { get; set; } = null!;

}

public class DashboardSection
{
    public int Id { get; set; }
    [Required]
    public required string Name { get; set; }
    public List<DashboardItem> Items { get; set; } = [];
}