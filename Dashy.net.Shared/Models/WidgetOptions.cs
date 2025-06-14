namespace Dashy.Net.Shared.Models;

/// <summary>
/// A marker interface for all widget-specific option types.
/// </summary>
public interface IWidgetOptions { }

/// <summary>
/// Options for the Static Link widget.
/// </summary>
public record StaticLinkOptions : IWidgetOptions
{
    public string? Url { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Options for the Clock widget. It has no configurable options.
/// </summary>
public record ClockOptions : IWidgetOptions;