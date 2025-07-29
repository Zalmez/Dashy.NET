using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Dashy.Net.ApiService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dashy.Net.Tests.Unit.Controllers;

public class ItemsControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ItemsController _controller;
    private readonly Mock<ILogger<ItemsController>> _mockLogger;

    public ItemsControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockLogger = new Mock<ILogger<ItemsController>>();
        _controller = new ItemsController(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateItem_WithValidDto_CreatesSuccessfully()
    {
        var dashboard = new Dashboard { Title = "Test Dashboard" };
        _context.Dashboards.Add(dashboard);
        await _context.SaveChangesAsync();

        var section = new DashboardSection
        {
            Name = "Test Section",
            DashboardId = dashboard.Id,
            Dashboard = dashboard
        };
        _context.Sections.Add(section);
        await _context.SaveChangesAsync();

        var createDto = new CreateItemDto
        {
            Title = "Test Item",
            Icon = "fas fa-test",
            Widget = "static-link",
            SectionId = section.Id
        };

        var result = await _controller.CreateItem(createDto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var itemVm = Assert.IsType<ItemVm>(okResult.Value);
        Assert.Equal("Test Item", itemVm.Title);
        Assert.Equal("fas fa-test", itemVm.Icon);
        Assert.Equal("static-link", itemVm.Widget);
        Assert.Equal(section.Id, itemVm.SectionId);
    }

    [Fact]
    public async Task CreateItem_WithInvalidSectionId_ReturnsBadRequest()
    {
        var createDto = new CreateItemDto
        {
            Title = "Test Item",
            SectionId = 999,
            Widget = "static-link"
        };

        var result = await _controller.CreateItem(createDto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Section not found.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateItem_WithValidData_UpdatesSuccessfully()
    {
        var dashboard = new Dashboard { Title = "Test Dashboard" };
        _context.Dashboards.Add(dashboard);
        await _context.SaveChangesAsync();

        var section = new DashboardSection
        {
            Name = "Test Section",
            DashboardId = dashboard.Id,
            Dashboard = dashboard
        };
        _context.Sections.Add(section);
        await _context.SaveChangesAsync();

        var item = new DashboardItem
        {
            Title = "Original Title",
            Icon = "fas fa-original",
            Widget = "static-link",
            SectionId = section.Id,
            Section = section
        };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateItemDto
        {
            Title = "Updated Title",
            Icon = "fas fa-updated",
            Widget = "weather-widget",
            SectionId = section.Id
        };

        var result = await _controller.UpdateItem(item.Id, updateDto);

        Assert.IsType<NoContentResult>(result);

        var updatedItem = await _context.Items.FindAsync(item.Id);
        Assert.NotNull(updatedItem);
        Assert.Equal("Updated Title", updatedItem.Title);
        Assert.Equal("fas fa-updated", updatedItem.Icon);
        Assert.Equal("weather-widget", updatedItem.Widget);
    }

    [Fact]
    public async Task UpdateItem_WithInvalidId_ReturnsNotFound()
    {
        var updateDto = new UpdateItemDto
        {
            Title = "Updated Title",
            SectionId = 1,
            Widget = "static-link"
        };

        var result = await _controller.UpdateItem(999, updateDto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteItem_WithValidId_DeletesSuccessfully()
    {
        var dashboard = new Dashboard { Title = "Test Dashboard" };
        _context.Dashboards.Add(dashboard);
        await _context.SaveChangesAsync();

        var section = new DashboardSection
        {
            Name = "Test Section",
            DashboardId = dashboard.Id,
            Dashboard = dashboard
        };
        _context.Sections.Add(section);
        await _context.SaveChangesAsync();

        var item = new DashboardItem
        {
            Title = "Item to Delete",
            SectionId = section.Id,
            Section = section
        };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteItem(item.Id);

        Assert.IsType<NoContentResult>(result);

        var deletedItem = await _context.Items.FindAsync(item.Id);
        Assert.Null(deletedItem);
    }

    [Fact]
    public async Task DeleteItem_WithInvalidId_ReturnsNotFound()
    {
        var result = await _controller.DeleteItem(999);

        Assert.IsType<NotFoundResult>(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
