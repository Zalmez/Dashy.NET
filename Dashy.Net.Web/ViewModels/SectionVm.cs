namespace Dashy.Net.Web.ViewModels;

public class SectionVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ItemVm> Items { get; set; } = new();
}

public class ItemVm
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Widget { get; set; }
    public int SectionId { get; set; }
    public Dictionary<string, object>? Options { get; set; }
}

