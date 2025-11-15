using Dashy.Net.Web.ViewModels;

namespace Dashy.Net.Web.Services;

/// <summary>
/// Service for managing application themes and density settings.
/// Supports Light, Dark, and High Contrast themes with optional density modes.
/// </summary>
public class ThemeService
{
    public enum Density
    {
        Comfortable,
        Compact
    }

    public List<Theme> AvailableThemes { get; } =
    [
        new() { Name = "Dark", CssClass = "theme-dark" },
        new() { Name = "Light", CssClass = "theme-light" },
        new() { Name = "High Contrast", CssClass = "theme-high-contrast" }
    ];

    public Theme CurrentTheme { get; private set; }
    public Density CurrentDensity { get; private set; } = Density.Comfortable;

    public event Action? OnThemeChanged;
    public event Action? OnDensityChanged;

    public ThemeService()
    {
        CurrentTheme = AvailableThemes.First();
    }

    /// <summary>
    /// Sets the current theme and notifies subscribers.
    /// </summary>
    public void SetTheme(string themeClass)
    {
        var newTheme = AvailableThemes.FirstOrDefault(t => t.CssClass == themeClass) ?? CurrentTheme;
        if (CurrentTheme.CssClass != newTheme.CssClass)
        {
            CurrentTheme = newTheme;
            OnThemeChanged?.Invoke();
        }
    }

    /// <summary>
    /// Sets the current theme without notifying subscribers.
    /// Used during initialization to avoid triggering events.
    /// </summary>
    public void SetCurrentThemeWithoutNotification(string themeClass)
    {
        CurrentTheme = AvailableThemes.FirstOrDefault(t => t.CssClass == themeClass) ?? CurrentTheme;
    }

    /// <summary>
    /// Sets the density mode and notifies subscribers.
    /// </summary>
    public void SetDensity(Density density)
    {
        if (CurrentDensity != density)
        {
            CurrentDensity = density;
            OnDensityChanged?.Invoke();
        }
    }

    /// <summary>
    /// Sets the density mode without notifying subscribers.
    /// </summary>
    public void SetDensityWithoutNotification(Density density)
    {
        CurrentDensity = density;
    }

    /// <summary>
    /// Gets the CSS class for the current density.
    /// </summary>
    public string GetDensityCssClass()
    {
        return CurrentDensity == Density.Compact ? "density-compact" : "density-comfortable";
    }
}