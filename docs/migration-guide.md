# Migration Guide: UI Redesign

## Overview

This guide covers migrating to the redesigned Dashy.NET UI. The redesign maintains **complete backwards compatibility** - no manual database migration steps or configuration changes are required (EF Core migrations run automatically on API startup).

## What's New

### Design System
- **Three Themes**: Light, Dark, and High Contrast
- **Density Modes**: Comfortable and Compact layouts
- **Design Tokens**: Centralized styling via CSS variables
- **Tailwind CSS**: Utility-first CSS framework
- **Fluent UI**: Modern component library from Microsoft

### User-Facing Features
- Enhanced theme switching with persistence
- Density toggle for different information densities
- Improved accessibility (WCAG 2.2 AA compliant)
- Better keyboard navigation
- Reduced motion support for accessibility

### Developer Features
- Primitive components wrapping Fluent UI
- Comprehensive design token system
- Better TypeScript/CSS tooling
- Widget integration guide
- Improved documentation

## Breaking Changes

**None.** This is a non-breaking update.

### Confirmed Compatibility
- ✅ All existing dashboards work without modification
- ✅ All existing widgets work without modification
- ✅ Database schema unchanged (no manual migration step required)
- ✅ API contracts unchanged
- ✅ Configuration format unchanged
- ✅ Authentication flow unchanged

## Installation / Update Steps

### For End Users

No action required! The UI will automatically update when you deploy the new version.

#### Optional: Update Preferences

After update, you can:
1. Visit your dashboard
2. Open settings (gear icon)
3. Choose your preferred theme (Light/Dark/High Contrast)
4. Choose your preferred density (Comfortable/Compact)

Your preferences are saved automatically.

### For Developers

#### Prerequisites
- .NET 10 SDK
- Node.js 18+ (for Tailwind CSS build)

#### Building from Source

1. **Clone/Pull the Repository**
   ```bash
   git clone https://github.com/Zalmez/Dashy.NET.git
   cd Dashy.NET
   git checkout copilot/ui-rework-for-dashy
   ```
2. **Install Node Dependencies**
   ```bash
   cd Dashy.Net.Web
   npm install
   ```
3. **Build Tailwind CSS**
   ```bash
   npm run css:build
   ```
4. **Build .NET Project**
   ```bash
   cd ..
   dotnet build
   ```
5. **Run**
   ```bash
   dotnet run --project Dashy.Net.AppHost
   ```

#### Development Workflow

During development, run CSS in watch mode:
```bash
cd Dashy.Net.Web
npm run css:watch
```

This automatically rebuilds CSS when you change `Styles/` files or Razor components.

#### CI/CD Integration

Update your build pipeline to include:
```yaml
# Example GitHub Actions
- name: Setup Node.js
  uses: actions/setup-node@v3
  with:
    node-version: '18'

- name: Install Node dependencies
  run: npm install
  working-directory: ./Dashy.Net.Web

- name: Build Tailwind CSS
  run: npm run css:build
  working-directory: ./Dashy.Net.Web

- name: Build .NET
  run: dotnet build
```

Or for standalone projects, add to `.csproj`:
```xml
<Target Name="BuildTailwindCSS" BeforeTargets="BeforeBuild">
  <Exec Command="npm run css:build" WorkingDirectory="$(ProjectDir)" />
</Target>
```

## Customization

### Changing Themes

Themes are defined in `/Dashy.Net.Web/Styles/tokens.css`. You can:

1. **Modify existing themes** by editing CSS variables:
   ```css
   body.theme-dark {
     --accent-color: #your-color;
     --background-color: #your-bg;
   }
   ```
2. **Add new themes**:
   ```css
   body.theme-custom {
     --accent-color: #ff6b6b;
     --background-color: #1a1a2e;
     /* ... other tokens */
   }
   ```
   Then register in `ThemeService.cs`:
   ```csharp
   public List<Theme> AvailableThemes { get; } =
   [
       new() { Name = "Dark", CssClass = "theme-dark" },
       new() { Name = "Light", CssClass = "theme-light" },
       new() { Name = "High Contrast", CssClass = "theme-high-contrast" },
       new() { Name = "Custom", CssClass = "theme-custom" }
   ];
   ```

### Customizing Components

All Fluent UI components are wrapped in our primitives (`/Components/Primitives/`). To customize:
1. Edit the primitive component (e.g., `Button.razor`)
2. Modify styling, behavior, or props
3. Changes automatically apply to all usage throughout the app

### Adding Icons

The app uses Font Awesome by default. To add more icon libraries:
1. **Lucide Icons** (recommended for new components):
   ```bash
   npm install lucide
   ```
2. **Fluent UI Icons**: Already included with `Microsoft.FluentUI.AspNetCore.Components`
3. Use in components:
   ```razor
   <i class="fas fa-star"></i>
   ```

## Rollback Plan

### If Issues Arise

The redesign is isolated in CSS and component wrappers. To roll back:

1. **Quick Rollback** (revert to old CSS):
   ```bash
   # In Dashy.Net.Web/Components/App.razor
   # Comment out:
   # <link rel="stylesheet" href="@Assets["css/app.generated.css"]" />
   ```
2. **Full Rollback** (revert entire PR):
   ```bash
   git revert <commit-range>
   git push
   ```
3. **Hybrid Approach** (keep new features, use old styling):
   - Keep ThemeService enhancements
   - Revert primitive components
   - Use old CSS

### Data Safety

All data is safe because:
- No database schema changes requiring manual steps
- No configuration format changes
- No API changes
- Theme preferences stored separately in browser localStorage

## Testing

### Before Deployment
1. Visual testing (themes, densities, devices)
2. Functional testing (widgets, CRUD, edit mode)
3. Accessibility testing (Axe, keyboard navigation, screen readers)
4. Performance testing (Lighthouse)

### After Deployment
1. Monitor logs and feedback
2. Iterate on accessibility and UI polish

## Common Migration Scenarios

### Scenario 1: Existing Production Installation

**Steps**:
1. Backup database (optional)
2. Deploy new version
3. No manual migration commands needed
4. Users get new UI automatically

### Scenario 2: Development Environment

**Steps**:
1. Pull latest changes
2. `npm install` in `Dashy.Net.Web`
3. `npm run css:build` in `Dashy.Net.Web`
4. `dotnet build` in root
5. Run and test

### Scenario 3: Custom Widgets

**Impact**: No changes required if widgets inherit from `WidgetBase`

**Recommendations**:
1. Use design tokens
2. Add Skeleton states
3. Test in all themes

## Support

### Getting Help
- Documentation: `/docs/ui-redesign.md`
- Widget Guide: `/docs/widget-integration-guide.md`
- Issues: GitHub Issues

### Reporting Issues
Include:
- Theme + density
- Browser/version
- Screenshot (if visual)
- Console errors (if functional)

## FAQ

(Selected items retained for brevity.)

### Q: Will my custom CSS break?
Selectors relying on removed Bootstrap classes may need updates.

### Q: Do I need to rebuild my widgets?
No; updating them to use tokens is optional but recommended.

### Q: Can I opt-out of the new UI?
No classic mode; customize themes via tokens.

## Checklist (Pre-Production)
- [ ] Build & test
- [ ] Staging validation
- [ ] Theme & density test
- [ ] Accessibility audit
- [ ] Mobile responsiveness
- [ ] Rollback plan verified
- [ ] User communication (optional)

## Timeline (Suggested)
1–4 weeks staged rollout depending on feedback.

## Additional Resources
- [UI Redesign Documentation](./ui-redesign.md)
- [Widget Integration Guide](./widget-integration-guide.md)
- [Tailwind CSS Docs](https://tailwindcss.com/)
- [Fluent UI Blazor Docs](https://www.fluentui-blazor.net/)
- [WCAG 2.2 Guidelines](https://www.w3.org/WAI/WCAG22/quickref/)
