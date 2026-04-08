---
sidebar_position: 5
---

# Widgets

Widgets are dynamic components that display real-time information on your Dashy.NET dashboard. They provide live data updates and interactive features beyond simple static links.

## Available Widgets

### Link Shortcut (`link`)

A clickable button that opens a URL. Ideal for quick navigation to your most-used services.

**Configuration:**
- **URL**: Target link
- **Description**: Optional subtitle shown on the button
- **Open in new tab**: Whether to open the link in a new browser tab

### Link Grid (`link-grid`)

A compact favicon grid of bookmarked URLs, perfect for organizing many links in a small space.

**Configuration:**
- **Links**: List of URL and label pairs

### System Stats (`system-stats`)

Displays live system performance metrics fetched from the API every 10 seconds.

**Features:**
- Memory usage (used / total)
- CPU uptime
- Process count
- Processor count

### Weather (`weather`)

Shows current weather conditions for a specified location using [Open-Meteo](https://open-meteo.com/) (no API key required).

**Configuration:**
- **Location**: City name (e.g. "London" or "New York")
- **Units**: `celsius` or `fahrenheit`

### Markdown Notes (`markdown`)

A full-featured markdown editor and viewer. Write notes, checklists, or any rich text content directly on your dashboard.

**Configuration:**
- **Content**: Markdown text (editable in-place on the dashboard)

### Status Checks (`status`)

Monitors the health of services by making HTTP requests and displaying their status.

**Configuration:**
- **Services**: List of service names and URLs to monitor

### Media Player (`media`)

Music and media playback controls widget.

### RSS Feed (`rss`)

Fetches and displays the latest items from an RSS or Atom feed, with optional auto-scroll.

**Configuration:**
- **Feed URL**: RSS or Atom feed URL
- **Limit**: Number of items to display (maximum 20)
- **Auto-scroll**: Automatically scroll through items

### CVE Tracker (`cve`)

Tracks recent security vulnerabilities from the [NVD](https://nvd.nist.gov/) (National Vulnerability Database) or [EUVD](https://euvd.enisa.europa.eu/) (EU Vulnerability Database).

**Configuration:**
- **Source**: `nvd` or `euvd`
- **Keyword**: Filter vulnerabilities by keyword (optional)
- **Limit**: Number of CVEs to display

### Section Drawer (`section`)

A collapsible container that groups other widgets together. Sections can be nested one level deep.

### API Widget (`api`)

Renders custom HTML and CSS content that is pushed to the widget via the REST API. Perfect for automation scripts or external services that need to display dynamic content on your dashboard.

**Configuration:**
- **Content**: HTML content (pushed via `PUT /api/widgets/{id}/content`)
- **CSS**: Optional scoped CSS styles

## Adding Widgets

### Through the UI

1. **Enter Edit Mode**: Click the edit button on your dashboard
2. **Click the "+" button**: In the section where you want to add a widget
3. **Choose Type**: Select from available widget types
4. **Configure**: Set widget-specific options
5. **Save**: Apply changes to your dashboard

### Widget Configuration

Each widget has common and specific configuration options:

#### Common Options
- **Title**: Display name for the widget
- **Icon**: Optional icon (Lucide icon name)
- **Size**: Widget dimensions (Small, Medium, Large)

## Widget Development

Custom widgets are Blazor components added directly to the `dashy3.Web` project.

### Widget Structure

Each widget is a `.razor` component stored in `dashy3.Web/Components/Widgets/`. The component receives the `Widget` model and can access its configuration via `Widget.GetConfig<T>(key, defaultValue)`.

```razor
@* MyCustomWidget.razor *@
@inherits WidgetBase

<div class="p-4">
    <h3>@Widget.Title</h3>
    <p>@Widget.GetConfig("message", "Hello!")</p>
</div>
```

Register the new widget type in `dashy3.Web/Models/WidgetType.cs`:

```csharp
public const string MyCustom = "my-custom";

// Add to the All list:
new(MyCustom, "My Custom Widget", "star-icon", "A custom widget example"),
```

Then add a case to `WidgetContent.razor` to dispatch to your new component.

### Widget Best Practices

- **Performance**: Fetch data on `OnInitializedAsync` and refresh on a timer only when necessary
- **Error Handling**: Handle API failures gracefully and show a user-friendly error state
- **Responsiveness**: Ensure widgets work well at all three sizes (Small, Medium, Large)
- **Theming**: Use CSS variables from the design token system instead of hard-coded colours
- **Accessibility**: Include appropriate ARIA labels and ensure keyboard navigability

## API Widget Integration

The **API Widget** (`api`) is designed for external integrations. You can push HTML/CSS content to it programmatically using the REST API:

```bash
curl -X PUT https://your-dashy-instance/api/widgets/{widgetId}/content \
  -H "X-Api-Key: your-api-key" \
  -H "Content-Type: application/json" \
  -d '{"content": "<p>Hello from the API!</p>", "css": "p { color: green; }"}'
```

See the [API Reference](/docs/api-reference) for full details on authentication and available endpoints.

## Troubleshooting Widgets

### Common Issues

**Widget not updating**:
- Check refresh interval settings
- Verify network connectivity from the server to external APIs
- Review the browser console and server logs for errors

**Weather widget shows "Location not found"**:
- Check the spelling of the city name
- Try a more specific location (e.g. "London, UK")

**RSS widget shows no items**:
- Verify the feed URL is accessible from the server
- Ensure the URL points to a valid RSS or Atom feed

**API widget shows placeholder**:
- No content has been pushed yet; use `PUT /api/widgets/{id}/content` to send content

## Next Steps

- [Learn about theming](/docs/theming)
- [Explore configuration options](/docs/configuration)
- [View the developer API reference](/docs/api-reference)
