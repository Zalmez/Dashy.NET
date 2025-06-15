using Dashy.Net.Shared.Models;
using Dashy.Net.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Dashy.Net.Web.Components.Shared.Widgets;
public abstract class WidgetBase : ComponentBase, IDisposable
{
    [Inject]
    protected ViewOptionsService ViewOptions { get; set; } = default!;

    /// <summary>
    /// The core data for the widget. Every widget MUST have this parameter.
    /// </summary>
    [Parameter, EditorRequired]
    public ItemVm Item { get; set; } = default!;

    /// <summary>
    /// A convenience property to safely get an option from the item's dictionary.
    /// </summary>
    /// <param name="key">The option key to look for (case-insensitive).</param>
    /// <returns>The option value as a string, or null if not found.</returns>
    protected string? GetOption(string key)
    {
        if (Item.Options is null) return null;

        var dictKey = Item.Options.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));

        return dictKey is not null ? Item.Options[dictKey]?.ToString() : null;
    }

    protected override void OnInitialized()
    {
        ViewOptions.OnChange += StateHasChanged;
    }

    public virtual void Dispose()
    {
        ViewOptions.OnChange -= StateHasChanged;
    }
}