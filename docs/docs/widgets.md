---
sidebar_position: 5
---

# Widgets

Widgets are dynamic components that display real-time information on your Dashy.NET dashboard. They provide live data updates and interactive features beyond simple static links.

## Available Widgets

### Clock Widget

Display current time and date with customizable formatting.

**Features:**
- Multiple time zones support
- 12-hour or 24-hour format
- Date display options
- Customizable styling

**Configuration:**
- **Time Zone**: Select from available time zones
- **Format**: Choose 12-hour or 24-hour display
- **Show Date**: Toggle date visibility
- **Style**: Size and color customization

### Weather Widget

Show current weather conditions and forecasts.

**Features:**
- Current weather conditions
- Multi-day forecast
- Location-based weather data
- Temperature in Celsius or Fahrenheit

**Configuration:**
- **Location**: City name or coordinates
- **Units**: Metric or Imperial
- **API Key**: OpenWeatherMap API key (required)
- **Show Forecast**: Enable multi-day forecast

### System Status Widget

Monitor system health and performance metrics.

**Features:**
- CPU usage
- Memory consumption
- Disk space
- Network activity

**Configuration:**
- **Refresh Interval**: How often to update (seconds)
- **Show Details**: Display detailed metrics
- **Thresholds**: Warning levels for alerts

### RSS Feed Widget

Display recent posts from RSS feeds.

**Features:**
- Multiple feed support
- Customizable item count
- Link previews
- Publication dates

**Configuration:**
- **Feed URL**: RSS/Atom feed URL
- **Item Count**: Number of items to display
- **Show Dates**: Display publication dates
- **Open Links**: Same tab or new tab

## Adding Widgets

### Through the UI

1. **Enter Edit Mode**: Click the edit button on your dashboard
2. **Select Section**: Choose where to add the widget
3. **Add Widget**: Click "Add Widget" or the "+" button
4. **Choose Type**: Select from available widget types
5. **Configure**: Set widget-specific options
6. **Save**: Apply changes to your dashboard

### Widget Configuration

Each widget has common and specific configuration options:

#### Common Options
- **Title**: Display name for the widget
- **Size**: Widget dimensions (small, medium, large)
- **Position**: Where in the section to place the widget
- **Refresh Rate**: How often to update data

#### Widget-Specific Options
Each widget type has its own configuration parameters as detailed in the sections above.

## Widget Development

Dashy.NET supports custom widget development for advanced users.

### Widget Structure

Widgets in Dashy.NET are Blazor components that implement the `IWidgetDescriptor` interface:

```csharp
public interface IWidgetDescriptor
{
    string Name { get; }
    string Description { get; }
    Type ComponentType { get; }
    Dictionary<string, object> DefaultSettings { get; }
}
```

### Creating a Custom Widget

1. **Create Widget Component**:
   ```csharp
   @inherits WidgetBase
   
   <div class="custom-widget">
       <h3>@Title</h3>
       <p>@Data</p>
   </div>
   
   @code {
       [Parameter] public string Title { get; set; }
       private string Data { get; set; }
       
       protected override async Task OnInitializedAsync()
       {
           // Initialize widget data
           await LoadDataAsync();
       }
   }
   ```

2. **Create Widget Descriptor**:
   ```csharp
   public class CustomWidgetDescriptor : IWidgetDescriptor
   {
       public string Name => "Custom Widget";
       public string Description => "A custom widget example";
       public Type ComponentType => typeof(CustomWidget);
       
       public Dictionary<string, object> DefaultSettings => new()
       {
           { "title", "My Custom Widget" },
           { "refreshInterval", 30 }
       };
   }
   ```

3. **Register Widget**:
   ```csharp
   // In Program.cs or startup
   services.AddTransient<IWidgetDescriptor, CustomWidgetDescriptor>();
   ```

### Widget Best Practices

- **Performance**: Use appropriate refresh intervals
- **Error Handling**: Handle API failures gracefully  
- **Responsiveness**: Ensure widgets work on mobile devices
- **Accessibility**: Include proper ARIA labels
- **Styling**: Follow the dashboard theme system

## Widget API Integration

### External APIs

Many widgets require external API integration:

#### Weather APIs
- **OpenWeatherMap**: Free tier available
- **AccuWeather**: Comprehensive weather data
- **WeatherAPI**: Simple integration

#### System Monitoring
- **Prometheus**: Metrics collection
- **Grafana**: Visualization integration
- **Custom APIs**: Your own monitoring endpoints

#### Social Media
- **RSS Feeds**: Standard RSS/Atom support
- **Twitter API**: Tweet integration
- **GitHub API**: Repository information

### API Key Management

Store API keys securely:

1. **Environment Variables**: Set via Docker/system environment
2. **Configuration UI**: Store encrypted in database
3. **Key Vault**: Azure Key Vault or similar services

## Troubleshooting Widgets

### Common Issues

**Widget not updating**:
- Check refresh interval settings
- Verify API key validity
- Check network connectivity
- Review browser console for errors

**Widget display issues**:
- Clear browser cache
- Check responsive design settings
- Verify CSS conflicts

**API rate limiting**:
- Increase refresh intervals
- Use API key if available
- Check API documentation for limits

### Widget Logs

Enable widget logging for troubleshooting:

```csharp
// In appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Dashy.Net.Widgets": "Debug"
    }
  }
}
```

## Widget Gallery

### Community Widgets

The Dashy.NET community has created additional widgets:
- **Cryptocurrency Prices**: Live crypto market data
- **Stock Tickers**: Real-time stock information  
- **Calendar Events**: Google Calendar integration
- **Server Monitoring**: Server health dashboards

### Contributing Widgets

To contribute a widget to Dashy.NET:

1. **Develop**: Create your widget following the guidelines
2. **Test**: Ensure it works across different configurations
3. **Document**: Add configuration and usage documentation
4. **Submit**: Create a pull request with your widget

## Next Steps

- [Learn about theming](/docs/theming)
- [Explore configuration options](/docs/configuration)
- [View the developer API reference](/docs/api-reference)
