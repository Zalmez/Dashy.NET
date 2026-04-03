namespace dashy3.Web.Services;

public interface IThemeService
{
    bool IsDarkMode { get; }
    event Action? ThemeChanged;
    void ToggleTheme();
    void SetDarkMode(bool isDark);
}
