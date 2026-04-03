namespace dashy3.ApiService.Entities;

public class DashboardEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = "Dashboard";
    public string LayoutMode { get; set; } = "Auto";
    public int Order { get; set; }

    public List<WidgetEntity> Widgets { get; set; } = [];
    public string? OwnerId { get; set; }
    public string Visibility { get; set; } = "Private";
}
