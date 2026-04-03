namespace dashy3.ApiService.Entities;

public class WidgetEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string DashboardId { get; set; } = "";
    public DashboardEntity Dashboard { get; set; } = null!;

    /// <summary>
    /// If this widget is inside a section widget, this references the parent section's Id.
    /// Null means it's a top-level widget on the dashboard.
    /// </summary>
    public string? ParentWidgetId { get; set; }
    public WidgetEntity? ParentWidget { get; set; }

    /// <summary>Child widgets (only applicable when Type == "section").</summary>
    public List<WidgetEntity> ChildWidgets { get; set; } = [];

    public string Type { get; set; } = "";
    public string Title { get; set; } = "Widget";
    public string? Icon { get; set; }
    public string Size { get; set; } = "Medium";
    public int Order { get; set; }
    /// <summary>Stored as JSON text in the database.</summary>
    public string ConfigJson { get; set; } = "{}";
}
