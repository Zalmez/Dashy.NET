using Dashy.Net.Shared;
using System.Reflection;

namespace Dashy.Net.Web.Services;

public class WidgetRegistryService
{
    public IReadOnlyList<IWidgetDescriptor> AvailableWidgets { get; }

    public WidgetRegistryService()
    {
        AvailableWidgets = Assembly.GetAssembly(typeof(IWidgetDescriptor))!
            .GetTypes()
            .Where(t => typeof(IWidgetDescriptor).IsAssignableFrom(t) && !t.IsInterface)
            .Select(t => (IWidgetDescriptor)Activator.CreateInstance(t)!)
            .ToList();
    }

    public IWidgetDescriptor? GetDescriptor(string typeIdentifier)
    {
        return AvailableWidgets.FirstOrDefault(w => w.TypeIdentifier.Equals(typeIdentifier, StringComparison.OrdinalIgnoreCase));
    }
}