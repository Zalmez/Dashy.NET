namespace Dashy.Net.Shared.Models;

public class StaticLinkWidgetDescriptor : IWidgetDescriptor
{
    public string TypeIdentifier => "static-link";
    public string DisplayName => "Static Link";
    public string Icon => "fas fa-link";
    public Type? OptionsEditorComponent => null;
}

public class ClockWidgetDescriptor : IWidgetDescriptor
{
    public string TypeIdentifier => "clock";
    public string DisplayName => "Clock";
    public string Icon => "fas fa-clock";
    public Type? OptionsEditorComponent => null;
}