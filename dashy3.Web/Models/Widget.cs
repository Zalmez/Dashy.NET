namespace dashy3.Web.Models;

public enum WidgetSize { Small, Medium, Large }

public class Widget
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Type { get; set; } = WidgetType.Link;
    public string Title { get; set; } = "Widget";
    public string? Icon { get; set; }
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
    public Dictionary<string, object> Config { get; set; } = [];

    /// <summary>
    /// If this widget is inside a section widget, this references the parent section's Id.
    /// Null means it's a top-level widget on the dashboard.
    /// </summary>
    public string? ParentWidgetId { get; set; }

    public int Order { get; set; }
    public bool IsBeingEdited { get; set; }
    public string? EditedBy { get; set; }

    public T GetConfig<T>(string key, T defaultValue)
    {
        if (!Config.TryGetValue(key, out var value))
            return defaultValue;

        if (value is T typed)
            return typed;

        // After HTTP deserialization, values come back as JsonElement
        if (value is System.Text.Json.JsonElement element)
        {
            try
            {
                var opts = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
                return System.Text.Json.JsonSerializer.Deserialize<T>(element.GetRawText(), opts) ?? defaultValue;
            }
            catch { return defaultValue; }
        }

        return defaultValue;
    }
}
