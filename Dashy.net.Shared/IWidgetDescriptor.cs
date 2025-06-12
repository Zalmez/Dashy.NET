namespace Dashy.Net.Shared;

public interface IWidgetDescriptor
{
    string TypeIdentifier { get; }
    string DisplayName { get; }

    string Icon { get; }
    Type? OptionsEditorComponent { get; }
}