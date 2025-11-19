using Dashy.Net.Shared;
using Dashy.Net.Web.Components.Shared.Widgets.Options;

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
    public string Description => "Displays the current weather for a location.";
}

public class PublicIPWidgetDescriptor : IWidgetDescriptor
{
    public string TypeIdentifier => "public-ip";
    public string Name => "Public IP";
    public string Icon => "fas fa-network-wired";
}

public class RssWidgetDescriptor : IWidgetDescriptor
{
    public string TypeIdentifier => "rss";
    public string Name => "RSS Feed";
    public string Icon => "fas fa-rss";
}

public class VulnerabilityFeedDescriptor : IWidgetDescriptor
{
    public string TypeIdentifier => "vulnerabilityfeed";
    public string Name => "Vulnerability Feed";
    public string Icon => "fas fa-rss";
}

// New container widget descriptor
public class SectionContainerWidgetDescriptor : IWidgetDescriptor
{
    public string TypeIdentifier => "section-container";
    public string Name => "section-container";
    public string Icon => "fas fa-folder-open";
}