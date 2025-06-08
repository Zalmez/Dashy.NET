using Dashy.Net.Web.ViewModels;

namespace Dashy.Net.Web.Services;


public class ThemeService
{
    public List<Theme> AvailableThemes { get; } =
    [
        new() { Name = "Dark", CssClass = "theme-dark" },
        new() { Name = "Light", CssClass = "theme-light" }
    ];

    public Theme CurrentTheme { get; private set; }

    public event Action? OnThemeChanged;

    // The constructor must look exactly like this, with NO parameters inside the parentheses.
    public ThemeService()
    {
        CurrentTheme = AvailableThemes.First(t => t.CssClass == "theme-dark");
    }

    public void SetTheme(Theme theme)
    {
        CurrentTheme = theme;
        OnThemeChanged?.Invoke();
    }
}

