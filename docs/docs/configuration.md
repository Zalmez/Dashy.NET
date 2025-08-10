---
sidebar_position: 3
---

# Configuration

Dashy.NET is designed to be configured entirely through its web interface. Unlike the original Dashy's YAML-based configuration, all settings are stored in a PostgreSQL database and managed through the intuitive UI.

## Edit Mode

The primary way to configure Dashy.NET is through **Edit Mode**, which provides a user-friendly interface for customizing your dashboard.

### Entering Edit Mode

1. **Access your dashboard**: Navigate to your Dashy.NET instance
2. **Toggle Edit Mode**: Look for the edit button (usually a pencil icon) in the interface
3. **Make changes**: The interface will switch to edit mode, allowing you to modify your dashboard

### What You Can Edit

In Edit Mode, you can:
- Add, remove, and reorder sections
- Add, edit, and delete items within sections
- Configure widgets
- Modify layouts and themes
- Set up authentication (if enabled)

## Dashboard Structure

Your Dashy.NET dashboard is organized hierarchically:

```
Dashboard
├── Header Buttons
├── Section 1
│   ├── Item 1
│   ├── Item 2
│   └── Widget 1
├── Section 2
│   ├── Item 3
│   └── Item 4
└── Footer/Settings
```

## Sections

Sections are containers that group related items and widgets together.

### Creating a Section

1. Enter Edit Mode
2. Click "Add Section" or the "+" button
3. Configure the section properties:
   - **Name**: Display name for the section
   - **Display Mode**: How items should be displayed (grid, list, etc.)
   - **Items per row**: Number of items to display per row
   - **Section styling**: Color, size, and other visual properties

### Section Properties

| Property | Description | Options |
|----------|-------------|---------|
| Name | Section title displayed to users | Any text |
| Display Mode | How items are arranged | Grid, List |
| Items per row | Grid layout columns | 1-8 |
| Show heading | Whether to display section name | True/False |

## Items

Items are the individual links, bookmarks, or shortcuts in your dashboard.

### Adding an Item

1. Navigate to the desired section in Edit Mode
2. Click "Add Item" or the "+" button within the section
3. Fill in the item details:
   - **Title**: Display name for the item
   - **Description**: Optional subtitle or description
   - **URL**: The link the item should open
   - **Icon**: Visual icon to represent the item
   - **Target**: Where to open the link (same tab, new tab, etc.)

### Item Properties

| Property | Description | Example |
|----------|-------------|---------|
| Title | Item display name | "GitHub" |
| Description | Optional subtitle | "Source code repository" |
| URL | Target link | "https://github.com/Zalmez/Dashy.NET" |
| Icon | Visual representation | "github" or URL to image |
| Target | Link behavior | Same tab, new tab, modal |

### Icon Options

Dashy.NET supports multiple icon sources:
- **Simple Icons**: Use the icon name (e.g., "github", "docker", "microsoft")
- **Custom URLs**: Direct links to image files
- **Emoji**: Unicode emoji characters
- **Font Awesome**: FontAwesome icon classes

## Widgets

Widgets display dynamic information on your dashboard.

### Available Widgets

- **Clock**: Display current time and date
- **Weather**: Show weather information for a location
- **System Status**: Display server/system metrics
- **RSS Feed**: Show recent posts from RSS feeds

### Adding a Widget

1. In Edit Mode, navigate to the desired section
2. Click "Add Widget" 
3. Select the widget type
4. Configure widget-specific settings
5. Save the widget

### Widget Configuration

Each widget type has its own configuration options:

#### Clock Widget
- **Time format**: 12-hour or 24-hour
- **Show date**: Include date display
- **Time zone**: Specify time zone

#### Weather Widget
- **Location**: City or coordinates
- **Units**: Metric or Imperial
- **Show forecast**: Display multi-day forecast

## Header Buttons

Customize the buttons that appear in the dashboard header.

### Adding Header Buttons

1. Enter Edit Mode
2. Navigate to Header Button settings
3. Add new buttons with:
   - **Text**: Button label
   - **URL**: Target link
   - **Position**: Button order
   - **Icon**: Optional icon

## Themes and Styling

Customize the appearance of your dashboard.

### Theme Options

- **Light Theme**: Clean, bright appearance
- **Dark Theme**: Dark mode for reduced eye strain
- **Custom CSS**: Advanced styling options

### Layout Settings

- **Item size**: Small, medium, or large items
- **Item spacing**: Gap between items
- **Section spacing**: Gap between sections
- **Background**: Custom background images or colors

## Authentication

Configure user authentication and access control.

### Authentication Providers

Dashy.NET supports various authentication methods:
- **Local accounts**: Username/password stored in database
- **OAuth**: GitHub, Google, Microsoft, etc.
- **LDAP**: Enterprise directory integration
- **Disable authentication**: Open access (default)

### Setting Up Authentication

1. Enter Edit Mode
2. Navigate to Authentication settings
3. Choose your authentication provider
4. Configure provider-specific settings
5. Set access permissions

## Import/Export

### Exporting Configuration

You can export your dashboard configuration for backup or migration:
1. Go to Settings
2. Click "Export Configuration"
3. Save the JSON file

### Importing Configuration

To import a previously exported configuration:
1. Go to Settings
2. Click "Import Configuration"  
3. Select your JSON file
4. Confirm the import

## Advanced Configuration

For advanced users, some settings can be configured via environment variables or direct database access, but this is generally not recommended as the UI provides all necessary configuration options.

## Troubleshooting

### Changes Not Saving
- Ensure you're in Edit Mode
- Check browser console for JavaScript errors
- Verify database connectivity

### Icons Not Loading
- Check icon name spelling
- Verify custom icon URLs are accessible
- Ensure network access to icon CDNs

### Widgets Not Updating
- Check widget configuration settings
- Verify API keys (for weather, etc.)
- Check browser console for errors

## Next Steps

- [Learn about widget development](/docs/widgets)
- [Explore theming options](/docs/theming)
- [Set up authentication](/docs/authentication)
