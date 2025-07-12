namespace Dashy.Net.Shared;

public interface IWidgetDescriptor
{
    string TypeIdentifier { get; }
    string Name { get; }
    string Icon { get; }
}