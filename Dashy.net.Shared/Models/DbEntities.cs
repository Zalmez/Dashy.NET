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
    public int Position { get; set; } = 0;

}

public class DashboardSection
{
    public int Id { get; set; }
    [Required]
    public required string Name { get; set; }
    public string? Icon { get; set; }
    public List<DashboardItem> Items { get; set; } = [];
    public int Position { get; set; } = 0;
    public int DashboardId { get; set; }
    public required Dashboard Dashboard { get; set; } = null!;
}

public class Dashboard
{
    public int Id { get; set; }
    [Required]
    public required string Title { get; set; } = "My Dashboard";
    public string? Subtitle { get; set; }
    public List<DashboardSection> Sections { get; set; } = [];
    public List<HeaderButton> HeaderButtons { get; set; } = [];
}

public class HeaderButton
{
    public int Id { get; set; }
    [Required]
    public required string Text { get; set; }
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public int Position { get; set; }

    public int DashboardId { get; set; }
    public required Dashboard Dashboard { get; set; } = null!;
}