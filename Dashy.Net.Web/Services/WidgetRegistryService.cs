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
            .OrderBy(d => d.DisplayName)
            .ToList();
    }

    public IWidgetDescriptor? GetDescriptor(string typeIdentifier)
    {
        return AvailableWidgets.FirstOrDefault(w => w.TypeIdentifier.Equals(typeIdentifier, StringComparison.OrdinalIgnoreCase));
    }
}