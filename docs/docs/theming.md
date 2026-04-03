---
sidebar_position: 6
---

# Theming

Dashy.NET ships with three built-in themes and two density modes. All preferences are saved automatically in the browser.

## Built-in Themes

| Theme | Description |
|-------|-------------|
| **Dark** | Default dark theme with purple accents |
| **Light** | Clean, bright theme |
| **High Contrast** | WCAG AA-compliant high-contrast theme |

Switch themes from the **Settings** panel in the top navigation bar.

## Density Modes

| Mode | Description |
|------|-------------|
| **Comfortable** | More padding and spacing between elements (default) |
| **Compact** | Tighter layout showing more information at once |

Switch density from the **Settings** panel in the top navigation bar.

## Customizing Themes

Themes are implemented using CSS custom properties (variables) defined in `dashy3.Web/Styles/tokens.css`. Each theme is applied as a class on the `<body>` element.

### Modifying an Existing Theme

Edit the relevant class in `tokens.css`:

```css
body.theme-dark {
  --accent-color: #7c5cbf;
  --background-color: #0e0e10;
  /* … other tokens */
}
```

### Creating a Custom Theme

1. Add a new block to `tokens.css`:
   ```css
   body.theme-custom {
     --accent-color: #ff6b6b;
     --background-color: #1a1a2e;
     /* Copy and adjust other tokens from an existing theme */
   }
   ```

2. Register the theme in `dashy3.Web/Services/ThemeService.cs`:
   ```csharp
   public List<Theme> AvailableThemes { get; } =
   [
       new() { Name = "Dark",          CssClass = "theme-dark" },
       new() { Name = "Light",         CssClass = "theme-light" },
       new() { Name = "High Contrast", CssClass = "theme-high-contrast" },
       new() { Name = "Custom",        CssClass = "theme-custom" },
   ];
   ```

## Design Token System

All colours, spacing, and typography are driven by CSS variables. Use these tokens in custom widgets and components instead of hard-coded values so your styles adapt correctly across themes:

| Token | Usage |
|-------|-------|
| `--accent-color` | Primary brand/highlight colour |
| `--background-color` | Page background |
| `--surface-color` | Card and panel background |
| `--text-color` | Primary text |
| `--text-muted` | Secondary/muted text |
| `--border-color` | Borders and dividers |

## Accessibility

- **High Contrast** theme targets WCAG 2.2 AA compliance.
- **Reduced motion**: The UI respects the `prefers-reduced-motion` media query — animations are disabled for users who have enabled reduced motion in their OS settings.
- Keyboard navigation is supported throughout the application.

