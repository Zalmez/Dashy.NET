namespace dashy3.Web.Services;

public class ThemeService : IThemeService
{
    public bool IsDarkMode { get; private set; } = false;
    public event Action? ThemeChanged;

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        ThemeChanged?.Invoke();
    }

    public void SetDarkMode(bool isDark)
    {
        IsDarkMode = isDark;
        ThemeChanged?.Invoke();
    }
}
