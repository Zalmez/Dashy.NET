using Dashy.Net.Web.Services;
using Xunit;

namespace Dashy.Net.Tests.Unit.Services;

public class ViewOptionsServiceTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var service = new ViewOptionsService();

        Assert.False(service.IsEditMode);
        Assert.Equal(ItemSize.Medium, service.CurrentItemSize);
        Assert.Equal(string.Empty, service.SearchTerm);
        Assert.Equal(LayoutOrientation.Auto, service.CurrentLayout);
    }

    [Fact]
    public void ToggleEditMode_ChangesEditModeState()
    {
        var service = new ViewOptionsService();
        var changeNotified = false;

        service.OnChange += () => changeNotified = true;

        service.ToggleEditMode();

        Assert.True(service.IsEditMode);
        Assert.True(changeNotified);

        changeNotified = false;
        service.ToggleEditMode();

        Assert.False(service.IsEditMode);
        Assert.True(changeNotified);
    }

    [Fact]
    public void SetSearchTerm_UpdatesSearchTerm()
    {
        var service = new ViewOptionsService();
        var changeNotified = false;

        service.OnChange += () => changeNotified = true;

        service.SetSearchTerm("test");

        Assert.Equal("test", service.SearchTerm);
        Assert.True(changeNotified);
    }

    [Fact]
    public void SetItemSize_UpdatesItemSize()
    {
        var service = new ViewOptionsService();
        var changeNotified = false;

        service.OnChange += () => changeNotified = true;

        service.SetItemSize(ItemSize.Large);

        Assert.Equal(ItemSize.Large, service.CurrentItemSize);
        Assert.True(changeNotified);
    }

    [Fact]
    public void SetLayout_UpdatesLayout()
    {
        var service = new ViewOptionsService();
        var changeNotified = false;

        service.OnChange += () => changeNotified = true;

        service.SetLayout(LayoutOrientation.Vertical);

        Assert.Equal(LayoutOrientation.Vertical, service.CurrentLayout);
        Assert.True(changeNotified);
    }
}
