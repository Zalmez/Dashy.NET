using System.Text.Json;
using System.Text.Json.Serialization;

namespace dashy3.Web.Models;

[JsonConverter(typeof(SafeLayoutModeConverter))]
public enum LayoutMode { Auto, Horizontal, Vertical }

/// <summary>Deserializes any unknown layoutMode string as Auto instead of throwing.</summary>
public class SafeLayoutModeConverter : JsonConverter<LayoutMode>
{
    public override LayoutMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return Enum.TryParse<LayoutMode>(str, ignoreCase: true, out var result) ? result : LayoutMode.Auto;
    }

    public override void Write(Utf8JsonWriter writer, LayoutMode value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}

public class Dashboard
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = "Dashboard";
    public LayoutMode LayoutMode { get; set; } = LayoutMode.Auto;
    public List<Widget> Widgets { get; set; } = [];
    public string? OwnerId { get; set; }
    public string Visibility { get; set; } = "Private";

    /// <summary>"Owner" | "Editor" | "Viewer" | null (public view only). Set server-side per caller.</summary>
    public string? UserPermission { get; set; }

    /// <summary>Gets top-level widgets (not inside a section).</summary>
    public IEnumerable<Widget> TopLevelWidgets => Widgets.Where(w => w.ParentWidgetId is null);

    /// <summary>Gets child widgets for a given section widget.</summary>
    public IEnumerable<Widget> GetChildWidgets(string parentWidgetId) =>
        Widgets.Where(w => w.ParentWidgetId == parentWidgetId).OrderBy(w => w.Order);
}
