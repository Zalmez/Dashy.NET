# UI Redesign Documentation

## Overview

This document describes the in-place UI/UX modernization of Dashy.NET, completed without breaking changes to existing features or data.

## Design Philosophy

The redesign maintains the core identity of "Dashy" while modernizing the interface to be:
- **Fluent and Modern**: Clean, contemporary design patterns
- **Accessible**: WCAG 2.2 AA compliant with keyboard-first navigation
- **Themeable**: Multiple theme options with user customization
- **Responsive**: Works from 320px to ultra-wide displays
- **Performance-oriented**: Fast first render, lazy loading, and code splitting

## Technology Choices

### Component Library: Microsoft Fluent UI Blazor

**Rationale:**
1. **Native .NET Integration**: Built specifically for Blazor by Microsoft, ensuring excellent compatibility with .NET 10
2. **Accessibility First**: WCAG 2.2 AA compliance built-in with proper ARIA attributes and keyboard navigation
3. **Modern Design**: Implements Microsoft's Fluent Design System with contemporary visual language
4. **Active Maintenance**: Regular updates and long-term support from Microsoft
5. **Theming Support**: Excellent support for design tokens and custom themes via CSS variables
6. **Performance**: Optimized for Blazor's rendering model
7. **License Compatible**: MIT license compatible with our GNU AGPLv3 requirements

**Alternative Considered:**
- **MudBlazor**: Excellent library, but Material Design aesthetic diverges more from Dashy's identity. Fluent UI's design language is more neutral and customizable.

### Styling: Tailwind CSS v3

**Rationale:**
1. **Utility-First**: Rapid development without naming conflicts
2. **Tree-Shaking**: Only used utilities are included in final CSS
3. **Consistency**: Design tokens map directly to Tailwind's configuration
4. **Responsive Design**: Built-in responsive modifiers simplify multi-device support
5. **Small Bundle**: After purging, very small CSS footprint
6. **Customization**: Easy to extend with our design tokens

## Design Token System

All visual styling is centralized in `/Styles/tokens.css` with CSS variables:

### Theme Support

1. **Light Theme** (`theme-light`)
   - Clean, bright interface
   - Good contrast for daylight use
   - Subtle gradients and shadows

2. **Dark Theme** (`theme-dark`)
   - Low-light optimized
   - Reduced eye strain
   - Subtle glass morphism effects

3. **High Contrast Theme** (`theme-high-contrast`)
   - Maximum contrast for accessibility
   - Clear borders and distinct colors
   - Optimized for users with visual impairments

### Density Options

1. **Comfortable** (default)
   - Standard spacing
   - Easier to click/tap
   - Better for touch devices

2. **Compact**
   - Reduced spacing
   - More information density
   - Preferred for desktop power users

## Architecture

### Directory Structure

```
Dashy.Net.Web/
├── Styles/
│   ├── tokens.css          # Design tokens (colors, spacing, etc.)
│   └── tailwind.css        # Tailwind configuration
├── Components/
│   ├── Primitives/         # Wrapped Fluent UI components
│   ├── Shell/              # App shell (TopBar, SideNav, etc.)
│   ├── Dashboard/          # Dashboard-specific components
│   └── ...
└── wwwroot/
    └── css/
        └── app.generated.css  # Generated Tailwind CSS
```

### Component Wrapping Pattern

Fluent UI components are wrapped in our own primitives to:
1. Enforce consistent styling via design tokens
2. Abstract third-party dependencies
3. Add project-specific behaviors
4. Enable easier library swapping if needed

Example:
```csharp
// Components/Primitives/Button.razor
<FluentButton Appearance="@GetAppearance()"
              @onclick="OnClick"
              Disabled="@Disabled">
    @ChildContent
</FluentButton>

@code {
    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;
    // ... applies our tokens to Fluent's button
}
```

## Theming Implementation

### ThemeService Enhancement

The `ThemeService` now supports:
- Three themes (Light, Dark, High Contrast)
- Density toggling (Comfortable, Compact)
- Persistence to localStorage/cookies
- Event-based notification for reactive updates

```csharp
@inject ThemeService ThemeService

// Subscribe to theme changes
ThemeService.OnThemeChanged += HandleThemeChanged;
ThemeService.OnDensityChanged += HandleDensityChanged;

// Change theme
ThemeService.SetTheme("theme-dark");

// Change density
ThemeService.SetDensity(ThemeService.Density.Compact);
```

## Build Process

### CSS Generation

Tailwind CSS is processed during build:

```bash
# Development (with watch)
npm run css:watch

# Production build
npm run css:build
```

The generated CSS is output to `wwwroot/css/app.generated.css` and is excluded from source control.

### Integration with .NET Build

Add to `.csproj` to auto-build CSS:

```xml
<Target Name="BuildTailwindCss" BeforeTargets="Build">
  <Exec Command="npm run css:build" WorkingDirectory="$(ProjectDir)" />
</Target>
```

## Accessibility Features

1. **Keyboard Navigation**: All interactive elements accessible via keyboard
2. **Focus Indicators**: Clear focus-visible styles for keyboard users
3. **Screen Reader Support**: Proper ARIA labels and semantic HTML
4. **Reduced Motion**: Respects `prefers-reduced-motion` media query
5. **High Contrast**: Dedicated high-contrast theme
6. **Color Contrast**: All text meets WCAG AA contrast requirements

## Performance Optimizations

1. **CSS Tree-Shaking**: Only used Tailwind utilities included
2. **Lazy Loading**: Heavy widget payloads loaded on-demand
3. **Code Splitting**: Large components split into separate chunks
4. **SSR Fast First Render**: Server-side rendering for initial page load
5. **Icon Optimization**: Only load icons actually used

## Migration Notes

### No Breaking Changes

- All existing features preserved
- Database schema unchanged
- Existing dashboards continue to work
- Widget API unchanged

### New Features Available

- High Contrast theme option
- Density toggle for comfortable/compact views
- Enhanced keyboard navigation
- Improved accessibility

### For Developers

When adding new components:

1. Use design tokens from `tokens.css`
2. Wrap Fluent UI components in `Components/Primitives/`
3. Follow existing component patterns
4. Test with all three themes
5. Verify keyboard navigation works
6. Check accessibility with screen reader

### Widget Integration

All existing widgets work without modification. To update a widget for the new design:

1. Inherit from `WidgetBase` (already required)
2. Use Tailwind utilities for layout
3. Use design tokens for colors/spacing
4. Test in all themes and densities

Example:
```razor
<div class="card p-4 @GetDensityPadding()">
    <h3 class="text-xl font-semibold mb-2" style="color: var(--text-color);">
        @Item.Title
    </h3>
    <!-- Widget content -->
</div>
```

## Testing Strategy

1. **Visual Testing**: Screenshots in all themes and densities
2. **Accessibility**: Axe DevTools and Lighthouse audits
3. **Keyboard Navigation**: Manual testing of all interactive flows
4. **Responsive**: Test on mobile, tablet, desktop, ultra-wide
5. **Performance**: Lighthouse performance scores
6. **Cross-browser**: Chrome, Firefox, Safari, Edge

## Future Enhancements

- Command palette (Ctrl/Cmd+K) for global search and commands
- Improved empty states with onboarding flow
- Enhanced settings UI with live preview
- Additional theme options (Ayu, Nord, etc.)
- Widget marketplace/gallery

## Resources

- [Fluent UI Blazor Documentation](https://www.fluentui-blazor.net/)
- [Tailwind CSS Documentation](https://tailwindcss.com/)
- [WCAG 2.2 Guidelines](https://www.w3.org/WAI/WCAG22/quickref/)
- [Fluent Design System](https://www.microsoft.com/design/fluent/)
