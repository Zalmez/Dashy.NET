# Dashy.NET Web Layout Modernization Plan

## Objectives

- Update the dashboard shell (header, navigation, FAB, footer) to meet 2025 UX expectations while staying consistent with .NET Aspire architecture.
- Improve accessibility: keyboard navigation, ARIA semantics, color contrast, focus handling, and responsive behavior.
- Simplify layout CSS, removing inline styles and adopting scalable design tokens.
- Preserve existing widget/content functionality while enhancing visual polish.

## Constraints & References

- Follow repo conventions in `.github/copilot-instructions.md` (BFF pattern, widget descriptors location, avoid DynamicComponent, use ItemEditModel binding, etc.).
- Maintain compatibility with ThemeService light/dark themes and client-side state services.
- Avoid new third-party client libraries unless approved; prefer native Blazor + CSS solutions.
- Ensure changes remain AGPL-compliant and work under dotnet watch.

## Step-by-Step Refactor Plan

### 1. Analyze Current Layout Architecture

1. Document component responsibilities (`MainLayout`, `NavMenu`, `App.razor`, shared modals) and their dependencies on services.
2. Inventory global styles in `wwwroot/app.css` and component CSS isolation files to identify overrides.
3. Capture current screenshots (light/dark, mobile/desktop) for comparison.

### 2. Establish Updated Design System Tokens

1. Define updated design tokens (spacing, typography, radius, shadows, z-index) backed by light/dark theme variable definitions in `app.css`.
2. Introduce CSS custom properties for responsive sizing using `clamp()` and motion preferences (`@media (prefers-reduced-motion)`), leaving existing tokens intact until migration.
3. Provide guidance for consistent elevation/blur usage across components.

### 3. Refactor Layout Structure

1. Replace header markup with semantic structure (e.g., `<header role="banner">`, `<nav role="navigation">`).
2. Convert dashboard dropdown into accessible pattern: button with `aria-expanded`, list with `role="menu"` or `listbox`, keyboard support.
3. Move floating action button stack into a Blazor component with focus management and ARIA relationships.
4. Relocate footer inline styles into `MainLayout.razor.css`, redesign as responsive sticky/footer that respects safe areas.

### 4. Rebuild Navigation Experience

1. Convert `NavMenu` checkbox hack into stateful Blazor component (toggle `bool IsNavOpen`), add keyboard/ARIA attributes.
2. Align navigation links with actual routes (remove placeholder Counter/Weather if obsolete; otherwise modernize icons via SVG components or CSS mask).
3. Add responsive layouts: desktop shows persistent rail, tablet collapses, mobile uses overlay with focus trap.
4. Ensure menu toggle integrates with ThemeService state (light/dark icon adjustments) and respects `prefers-reduced-motion`.

### 5. Modernize Header Controls & Toolbar

1. Group layout/size controls into accessible segmented buttons; implement `role="radiogroup"` semantics.
2. Rework sizing toggles to support keyboard navigation and screen reader labels.
3. Ensure header content wraps gracefully on narrow screens via CSS grid/flex with `minmax` and `clamp()` spacing.
4. Update dropdown styles to a neutral theme with consistent surface tokens and motion.

### 6. Introduce Responsive & Adaptive Styling

1. Create new layout section in `app.css` or dedicated partial for header/nav/fab, relying on CSS custom properties defined earlier.
2. Implement container queries (where supported) or fallback breakpoints for header toolbars and dashboard cards.
3. Add motion-safe transitions using `@media (prefers-reduced-motion: reduce)` to disable large animations.
4. Verify background image handling with `MainLayout` pseudo-element, ensuring contrast/balance remains accessible.

### 7. Accessibility Enhancements

1. Review modals (`Modal`, `HeaderButtonEditor`, etc.) to ensure focus trapping and `aria-modal` compliance.
2. Apply `aria-live` or toast pattern for success/error states (logo upload, background change).
3. Validate color contrast for primary elements (buttons, text on blur backgrounds) against WCAG 2.2 AA.
4. Add keyboard shortcuts summary (optional) or ensure existing shortcuts are documented.

### 8. Clean Up CSS & Assets

1. Remove unused selectors from `MainLayout.razor.css` and `App.css`, keeping isolation boundaries.
2. Consolidate inline styles into CSS files; ensure consistent naming (BEM-like or utility tokens).
3. Optimize asset usage (logo sizing, background images) and ensure `images/` references leverage `FileStorageService` outputs.

### 9. Testing & Validation

1. Run `dotnet test` (existing suite) and add snapshot screenshots/manual QA checklist.
2. Execute accessibility audits (Lighthouse, axe, keyboard walkthrough) in light/dark themes and multiple viewport sizes.
3. Verify persisted client storage keys (`header_visible`, custom logo paths) still function post-refactor.
4. Update documentation (README or docs) with new UX capabilities and usage instructions.

### 10. Deployment Readiness

1. Finalize changelog entry summarizing structural/accessibility improvements.
2. Coordinate PR review focusing on UI/UX, accessibility, and regression testing.
3. Prepare for staggered release: flag potential feature toggles if required.

## Suggested Deliverables Per Milestone

- Design tokens + mockups (Figma or screenshots).
- Updated layout components with associated unit/component tests.
- Accessibility audit report.
- Documentation update and release notes draft.
