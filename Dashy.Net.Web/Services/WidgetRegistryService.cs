using Dashy.Net.Shared;
using System.Reflection;

namespace Dashy.Net.Web.Services;

public class WidgetRegistryService
{
    public IReadOnlyList<IWidgetDescriptor> AvailableWidgets { get; }

    public WidgetRegistryService()
    {
        AvailableWidgets = typeof(WidgetRegistryService).Assembly
            .GetTypes()
            .Where(t => typeof(IWidgetDescriptor).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(t => (IWidgetDescriptor)Activator.CreateInstance(t)!)
            .OrderBy(d => d.Name)
            .ToList();
    }

    /// <summary>
    /// Gets a widget descriptor by its unique name (e.g., "static-link").
    /// </summary>
    /// <param name="name">The unique identifier for the widget type.</param>
    public IWidgetDescriptor? GetDescriptor(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return AvailableWidgets.FirstOrDefault(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}