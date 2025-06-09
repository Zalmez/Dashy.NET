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

    public ThemeService()
    {
        CurrentTheme = AvailableThemes.First(t => t.CssClass == "theme-dark");
    }

    public void SetTheme(string themeClass)
    {
        var newTheme = AvailableThemes.FirstOrDefault(t => t.CssClass == themeClass) ?? CurrentTheme;
        if (CurrentTheme.CssClass != newTheme.CssClass)
        {
            CurrentTheme = newTheme;
            OnThemeChanged?.Invoke();
        }
    }
}