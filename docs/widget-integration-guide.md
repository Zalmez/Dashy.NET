# Widget Integration Guide

## Overview

This guide explains how to create and integrate widgets with the redesigned Dashy.NET UI. All widgets must inherit from `WidgetBase` and follow the established patterns for consistency.

## Widget Architecture

### Required Base Class

All widgets **MUST** inherit from `WidgetBase`:

```csharp
@inherits WidgetBase

// Your widget markup and code
```

The `WidgetBase` class provides:
- Access to `ViewOptionsService` for layout/size information
- Event subscription management
- The `Item` parameter (dashboard item data)
- Helper methods for options parsing
- Disposal pattern implementation
- `IsDisposed` flag for async safety

### Widget Structure

A typical widget follows this structure:

```razor
@inherits WidgetBase

<div class="card @GetItemSizeClass() p-4">
    <!-- Widget header -->
    <div class="flex items-center justify-between mb-4">
        <h3 class="text-lg font-semibold" style="color: var(--text-color);">
            @Item.Title
        </h3>
        @if (ViewOptions.IsEditMode)
        {
            <div class="widget-actions">
                <button @onclick="OnItemEdited" class="btn-ghost btn-sm">
                    <i class="fas fa-edit"></i>
                </button>
                <button @onclick="() => OnItemDeleted.InvokeAsync(Item)" class="btn-ghost btn-sm text-danger">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        }
    </div>

    <!-- Widget content -->
    <div class="widget-content">
        @if (_isLoading)
        {
            <Skeleton Height="4rem" />
        }
        else if (!string.IsNullOrEmpty(_error))
        {
            <div class="text-danger">@_error</div>
        }
        else
        {
            <!-- Your widget content here -->
        }
    </div>
</div>

@code {
    private bool _isLoading = true;
    private string? _error;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            // Load data
            await Task.Delay(1000); // Simulate API call
            if (IsDisposed) return;
            
            // Update state
            StateHasChanged();
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                _error = ex.Message;
            }
        }
        finally
        {
            if (!IsDisposed)
            {
                _isLoading = false;
                StateHasChanged();
            }
        }
    }
}
```

## Design Token Usage

### Colors

Always use CSS variables for colors to support theming:

```razor
<!-- Text colors -->
<span style="color: var(--text-color);">Main text</span>
<span style="color: var(--subtle-text-color);">Secondary text</span>

<!-- Background colors -->
<div style="background-color: var(--card-background-color);">Card</div>

<!-- Semantic colors -->
<span style="color: var(--accent-color);">Accent</span>
<span style="color: var(--danger-color);">Error</span>
<span style="color: var(--success-color);">Success</span>
<span style="color: var(--warning-color);">Warning</span>
```

### Tailwind Utilities

Use Tailwind classes for layout and spacing:

```razor
<!-- Layout -->
<div class="flex items-center justify-between">
<div class="grid grid-cols-2 gap-4">

<!-- Spacing -->
<div class="p-4 mb-2 mt-4">  <!-- padding, margin-bottom, margin-top -->
<div class="mx-auto">         <!-- margin x-axis auto (center) -->

<!-- Sizing -->
<div class="w-full h-32">
<div class="max-w-md">

<!-- Typography -->
<h1 class="text-2xl font-bold">
<p class="text-sm text-subtle">
```

## Widget Options

Access widget-specific configuration using `GetOption`:

```csharp
@code {
    private string? GetApiKey() => GetOption("apiKey");
    private string? GetLocation() => GetOption("location");
    private int GetRefreshInterval() => 
        int.TryParse(GetOption("refreshInterval"), out var val) ? val : 60;
}
```

Options are stored in `Item.Options` as a `JsonElement`.

## Async Operations Safety

**Critical**: Always check `IsDisposed` after `await`:

```csharp
private async Task RefreshDataAsync()
{
    var data = await _apiClient.GetDataAsync();
    if (IsDisposed) return;  // IMPORTANT!
    
    _data = data;
    StateHasChanged();
}
```

This prevents `ObjectDisposedException` when the component is disposed during an async operation.

## Loading States

Use the `Skeleton` primitive for loading states:

```razor
@if (_isLoading)
{
    <div class="space-y-2">
        <Skeleton Height="2rem" Width="100%" />
        <Skeleton Height="4rem" Width="100%" />
        <Skeleton Height="3rem" Width="80%" />
    </div>
}
```

## Error Handling

Display errors using design tokens:

```razor
@if (!string.IsNullOrEmpty(_error))
{
    <div class="p-4 rounded" style="background-color: var(--danger-color); color: white;">
        <div class="flex items-center gap-2">
            <i class="fas fa-exclamation-triangle"></i>
            <span>@_error</span>
        </div>
    </div>
}
```

## Responsive Design

Widgets automatically adapt to size via `GetItemSizeClass()`:

```csharp
<div class="card @GetItemSizeClass()">
    <!-- Content adapts based on: -->
    <!-- item-size-small, item-size-medium, or item-size-large -->
</div>
```

You can also add custom responsive behavior:

```razor
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
    <!-- Responsive grid -->
</div>
```

## Widget Registration

Register your widget in `WidgetRegistryService`:

```csharp
// In Dashy.Net.Web/Services/WidgetDescriptors.cs
public class MyWidgetDescriptor : IWidgetDescriptor
{
    public string Name => "My Widget";
    public string Key => "my-widget";
    public string Description => "Description of what the widget does";
    public string IconClass => "fas fa-star";
    
    public List<WidgetOption> GetOptions()
    {
        return new List<WidgetOption>
        {
            new() { Key = "apiKey", Label = "API Key", Type = "text", Required = true },
            new() { Key = "refreshInterval", Label = "Refresh (seconds)", Type = "number", DefaultValue = "60" }
        };
    }
}
```

## Accessibility

Ensure your widget is accessible:

```razor
<!-- Use semantic HTML -->
<button type="button" aria-label="Refresh widget" @onclick="RefreshAsync">
    <i class="fas fa-sync" aria-hidden="true"></i>
</button>

<!-- Provide text alternatives for icons -->
<i class="fas fa-info-circle" aria-label="Information"></i>

<!-- Use proper heading hierarchy -->
<h3>Widget Title</h3>
<h4>Subsection</h4>
```

## Testing Themes

Test your widget in all themes:
- Light theme (`theme-light`)
- Dark theme (`theme-dark`)
- High contrast theme (`theme-high-contrast`)

And both densities:
- Comfortable (default)
- Compact

## Example: Weather Widget

```razor
@inherits WidgetBase

<div class="card @GetItemSizeClass() p-4">
    <div class="flex items-center justify-between mb-4">
        <h3 class="text-lg font-semibold" style="color: var(--text-color);">
            <i class="fas fa-cloud-sun mr-2"></i>
            Weather
        </h3>
        @if (ViewOptions.IsEditMode)
        {
            <div class="flex gap-2">
                <button @onclick="OnItemEdited" class="btn-ghost btn-sm" aria-label="Edit widget">
                    <i class="fas fa-edit"></i>
                </button>
                <button @onclick="() => OnItemDeleted.InvokeAsync(Item)" 
                        class="btn-ghost btn-sm" 
                        style="color: var(--danger-color);"
                        aria-label="Delete widget">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        }
    </div>

    @if (_isLoading)
    {
        <div class="space-y-2">
            <Skeleton Height="3rem" />
            <Skeleton Height="2rem" Width="60%" />
        </div>
    }
    else if (_weather != null)
    {
        <div class="text-center">
            <div class="text-4xl font-bold mb-2" style="color: var(--text-color);">
                @_weather.Temperature°C
            </div>
            <div class="text-lg" style="color: var(--subtle-text-color);">
                @_weather.Condition
            </div>
            <div class="mt-4 grid grid-cols-2 gap-4">
                <div>
                    <div style="color: var(--subtle-text-color);">Humidity</div>
                    <div style="color: var(--text-color);">@_weather.Humidity%</div>
                </div>
                <div>
                    <div style="color: var(--subtle-text-color);">Wind</div>
                    <div style="color: var(--text-color);">@_weather.WindSpeed km/h</div>
                </div>
            </div>
        </div>
    }
    else if (!string.IsNullOrEmpty(_error))
    {
        <div class="p-3 rounded" style="background-color: var(--danger-color); color: white;">
            @_error
        </div>
    }
</div>

@code {
    private bool _isLoading = true;
    private string? _error;
    private WeatherData? _weather;
    private Timer? _refreshTimer;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadWeatherAsync();
        
        // Set up auto-refresh
        var interval = GetRefreshInterval();
        _refreshTimer = new Timer(async _ => await LoadWeatherAsync(), null, 
            TimeSpan.FromSeconds(interval), TimeSpan.FromSeconds(interval));
    }

    private async Task LoadWeatherAsync()
    {
        _isLoading = true;
        _error = null;
        
        try
        {
            var location = GetOption("location") ?? "New York";
            _weather = await FetchWeatherAsync(location);
            if (IsDisposed) return;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                _error = $"Failed to load weather: {ex.Message}";
            }
        }
        finally
        {
            if (!IsDisposed)
            {
                _isLoading = false;
                StateHasChanged();
            }
        }
    }

    private int GetRefreshInterval() => 
        int.TryParse(GetOption("refreshInterval"), out var val) ? val : 300;

    private async Task<WeatherData> FetchWeatherAsync(string location)
    {
        // Implementation
        await Task.Delay(1000);
        return new WeatherData();
    }

    public override void Dispose()
    {
        _refreshTimer?.Dispose();
        base.Dispose();
    }

    private class WeatherData
    {
        public double Temperature { get; set; }
        public string Condition { get; set; } = "";
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
    }
}
```

## Best Practices

1. **Always inherit from WidgetBase**
2. **Check IsDisposed after every await**
3. **Use design tokens for all colors**
4. **Provide loading states with Skeleton**
5. **Handle errors gracefully**
6. **Make widgets responsive**
7. **Ensure accessibility**
8. **Test in all themes and densities**
9. **Dispose resources properly**
10. **Use semantic HTML**

## Common Patterns

### Refresh Button

```razor
<button @onclick="RefreshAsync" 
        class="btn-ghost btn-sm" 
        disabled="@_isLoading"
        aria-label="Refresh">
    <i class="fas @(_isLoading ? "fa-spinner fa-spin" : "fa-sync")"></i>
</button>
```

### Status Badge

```razor
@if (_status != null)
{
    <Badge Variant="@GetStatusVariant()">
        @_status.Message
    </Badge>
}

@code {
    private Badge.BadgeVariant GetStatusVariant() => _status?.IsHealthy == true 
        ? Badge.BadgeVariant.Success 
        : Badge.BadgeVariant.Danger;
}
```

### Data List

```razor
<div class="space-y-2">
    @foreach (var item in _items)
    {
        <div class="flex items-center justify-between p-2 rounded hover:bg-opacity-10"
             style="background-color: var(--card-hover-background-color);">
            <span style="color: var(--text-color);">@item.Name</span>
            <span style="color: var(--subtle-text-color);">@item.Value</span>
        </div>
    }
</div>
```

## Troubleshooting

### Widget not showing up
- Ensure it's registered in `WidgetRegistryService`
- Check that the widget key matches in the database
- Verify the component is in the correct namespace

### Colors not updating with theme
- Use CSS variables (`var(--color-name)`) instead of hardcoded colors
- Don't use Tailwind color classes for theme-dependent colors

### ObjectDisposedException
- Always check `IsDisposed` after `await`
- Wrap state updates in `if (!IsDisposed) { ... }`

### Widget not refreshing
- Ensure you call `StateHasChanged()` after data updates
- Check that you're using `InvokeAsync` for event handlers from services
