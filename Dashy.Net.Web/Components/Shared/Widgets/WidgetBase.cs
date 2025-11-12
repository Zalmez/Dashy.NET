using Dashy.Net.Shared.Models;
using Dashy.Net.Web.Services;
using Dashy.Net.Web.Helpers;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace Dashy.Net.Web.Components.Shared.Widgets;

public abstract class WidgetBase : ComponentBase, IDisposable
{
    [Inject]
    protected ViewOptionsService ViewOptions { get; set; } = default!;

    [Inject]
    protected EventSubscriptionManager SubscriptionManager { get; set; } = default!;

    /// <summary>
    /// The core data for the widget. Every widget MUST have this parameter.
    /// </summary>
    [Parameter, EditorRequired]
    public ItemVm Item { get; set; } = default!;

    /// <summary>
    /// A flag to indicate if the component has been disposed.
    /// Any async method should check this before updating state.
    /// </summary>
    protected bool IsDisposed { get; private set; }

    /// <summary>
    /// Determines the CSS class name corresponding to the current item size.
    /// </summary>
    /// <returns>A string representing the CSS class name for the current item size.  The format is "item-size-" followed by the
    protected string GetItemSizeClass() => "item-size-" + ViewOptions.CurrentItemSize.ToString().ToLower();

    [Parameter] public EventCallback<ItemVm> OnItemDeleted { get; set; }
    [Parameter] public EventCallback OnItemEdited { get; set; }

    /// <summary>
    /// A convenience property to safely get an option from the item's dictionary.
    /// </summary>
    /// <param name="key">The option key to look for (case-insensitive).</param>
    /// <returns>The option value as a string, or null if not found.</returns>
    protected string? GetOption(string key)
    {
        if (Item.Options is not JsonElement element || element.ValueKind != JsonValueKind.Object)
            return null;

        if (TryGetPropertyIgnoreCase(element, key, out var prop))
        {
            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString(),
                JsonValueKind.Number => prop.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                _ => prop.ToString()
            };
        }
        return null;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        foreach (var prop in element.EnumerateObject())
        {
            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    protected override void OnInitialized()
    {
        SubscriptionManager.AddSubscription(() => ViewOptions.OnChange -= HandleViewOptionsChanged);
        ViewOptions.OnChange += HandleViewOptionsChanged;
    }

    private void HandleViewOptionsChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public virtual void Dispose()
    {
        SubscriptionManager.Dispose();
        IsDisposed = true;
    }
}