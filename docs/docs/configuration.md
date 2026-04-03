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

#### Weather Widget
- **Location**: City name (e.g. "London")
- **Units**: `celsius` or `fahrenheit`

#### RSS Widget
- **Feed URL**: RSS or Atom feed URL
- **Limit**: Number of items to display (max 20)

#### CVE Tracker Widget
- **Source**: `nvd` or `euvd`
- **Keyword**: Optional search term

## Themes and Styling

Customize the appearance of your dashboard from the **Settings** panel.

### Theme Options

- **Dark Theme**: Default dark theme with purple accents
- **Light Theme**: Clean, bright appearance
- **High Contrast**: WCAG AA-compliant high-contrast theme

See the [Theming guide](/docs/theming) for more details, including how to create custom themes.

## Authentication

Configure user authentication and access control.

### Authentication Methods

Dashy.NET supports the following authentication methods:
- **Local accounts**: Username and password stored in the database
- **OIDC/OAuth2**: Single sign-on via any OpenID Connect-compatible provider (Keycloak, Auth0, Azure AD, GitHub, Google, etc.)

Authentication is required by default. The first user to register becomes the Admin.

### Setting Up Authentication

See the [Authentication guide](/docs/authentication) for detailed setup instructions.

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
