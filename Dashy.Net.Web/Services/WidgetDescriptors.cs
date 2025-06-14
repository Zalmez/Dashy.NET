using Dashy.Net.Shared;
using Dashy.Net.Web.Components.Shared.Widgets;

namespace Dashy.Net.Web.Widgets;

public class StaticLinkWidgetDescriptor : IWidgetDescriptor
{
    public string TypeIdentifier => "static-link";
    public string DisplayName => "Static Link";
    public string Icon => "fas fa-link";
    public Type? OptionsEditorComponent => typeof(StaticLinkOptionsEditor);
}

public class ClockWidgetDescriptor : IWidgetDescriptor
{
    public string TypeIdentifier => "clock";
    public string DisplayName => "Clock";
    public string Icon => "fas fa-clock";
    // The clock has no special options, so its editor is null.
    public Type? OptionsEditorComponent => null;
}