using Dashy.Net.Shared;

namespace Dashy.Net.Web.Services;

public class StaticLinkWidgetDescriptor : IWidgetDescriptor
{
    public string TypeIdentifier => "static-link";
    public string DisplayName => "Static Link";
    public string Name => "static-link";
    public string Icon => "fas fa-link";
    public Type? OptionsEditorComponent => null;
}

public class ClockWidgetDescriptor : IWidgetDescriptor
{
    public string TypeIdentifier => "clock";
    public string Name => "clock";
    public string Icon => "fas fa-clock";
    public Type? OptionsEditorComponent => null;
}

public class WeatherWidgetDescriptor : IWidgetDescriptor
{
    public string TypeIdentifier => "weather";
    public string Icon => "fas fa-cloud-sun";
    public string Name => "weather";
    public string DisplayName => "Weather";
    public string Description => "Displays the current weather for a location.";
    public Type? OptionsEditorComponent => null;
}
