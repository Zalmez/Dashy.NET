using Dashy.Net.Web.Services;
using Xunit;

namespace Dashy.Net.Tests.Unit.Services;

public class ThemeServiceTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultTheme()
    {
        var service = new ThemeService();

        Assert.NotNull(service.CurrentTheme);
        Assert.Equal("theme-dark", service.CurrentTheme.CssClass);
        Assert.NotEmpty(service.AvailableThemes);
    }

    [Fact]
    public void SetTheme_WithValidTheme_UpdatesCurrentTheme()
    {
        var service = new ThemeService();
        var themeChanged = false;

        service.OnThemeChanged += () => themeChanged = true;

        service.SetTheme("theme-light");

        Assert.Equal("theme-light", service.CurrentTheme.CssClass);
        Assert.True(themeChanged);
    }

    [Fact]
    public void SetTheme_WithInvalidTheme_KeepsCurrentTheme()
    {
        var service = new ThemeService();
        var originalTheme = service.CurrentTheme;
        var themeChanged = false;

        service.OnThemeChanged += () => themeChanged = true;

        service.SetTheme("invalid-theme");

        Assert.Equal(originalTheme.CssClass, service.CurrentTheme.CssClass);
        Assert.False(themeChanged);
    }

    [Fact]
    public void SetCurrentThemeWithoutNotification_DoesNotTriggerEvent()
    {
        var service = new ThemeService();
        var themeChanged = false;

        service.OnThemeChanged += () => themeChanged = true;

        service.SetCurrentThemeWithoutNotification("theme-light");

        Assert.Equal("theme-light", service.CurrentTheme.CssClass);
        Assert.False(themeChanged);
    }

    [Fact]
    public void AvailableThemes_ContainsExpectedThemes()
    {
        var service = new ThemeService();

        Assert.Equal(2, service.AvailableThemes.Count);
        Assert.Contains(service.AvailableThemes, t => t.CssClass == "theme-dark");
        Assert.Contains(service.AvailableThemes, t => t.CssClass == "theme-light");
    }

    [Fact]
    public void SetTheme_WithSameTheme_DoesNotTriggerEvent()
    {
        var service = new ThemeService();
        var initialTheme = service.CurrentTheme.CssClass;
        var themeChanged = false;

        service.OnThemeChanged += () => themeChanged = true;

        service.SetTheme(initialTheme);

        Assert.False(themeChanged);
    }

    [Fact]
    public void AvailableThemes_ContainsValidThemes()
    {
        var service = new ThemeService();

        foreach (var theme in service.AvailableThemes)
        {
            Assert.NotNull(theme.Name);
            Assert.NotEmpty(theme.Name);
            Assert.NotNull(theme.CssClass);
            Assert.NotEmpty(theme.CssClass);
        }
    }
}