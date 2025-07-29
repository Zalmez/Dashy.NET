using Dashy.Net.ApiService.Controllers;
using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dashy.Net.Tests.Unit.Controllers;

public class DashboardControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DashboardController _controller;
    private readonly Mock<ILogger<DashboardController>> _loggerMock;

    public DashboardControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _loggerMock = new Mock<ILogger<DashboardController>>();
        _controller = new DashboardController(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetConfig_WithExistingDashboard_ReturnsConfig()
    {
        var dashboard = new Dashboard
        {
            Title = "Test Dashboard",
            Subtitle = "Test Subtitle",
            Sections = new List<DashboardSection>
            {
                new DashboardSection { Name = "Test Section", Position = 0 }
            }
        };
        _context.Dashboards.Add(dashboard);
        await _context.SaveChangesAsync();

        var result = await _controller.GetConfig();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetConfig_WithNoDashboards_ReturnsNotFound()
    {
        var result = await _controller.GetConfig(id: 999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetDashboardList_ReturnsAllDashboards()
    {
        var dashboard1 = new Dashboard { Title = "Dashboard 1" };
        var dashboard2 = new Dashboard { Title = "Dashboard 2" };
        _context.Dashboards.AddRange(dashboard1, dashboard2);
        await _context.SaveChangesAsync();

        var result = await _controller.GetDashboardList();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task CreateDashboard_WithValidData_ReturnsCreated()
    {
        var dashboardDto = new CreateDashboardDto
        {
            Title = "New Dashboard",
            Subtitle = "New Subtitle"
        };

        var result = await _controller.CreateDashboard(dashboardDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(DashboardController.GetConfig), createdResult.ActionName);

        var dashboard = await _context.Dashboards.FirstOrDefaultAsync(d => d.Title == "New Dashboard");
        Assert.NotNull(dashboard);
    }

    [Fact]
    public async Task UpdateDashboard_WithValidData_ReturnsOk()
    {
        var dashboard = new Dashboard { Title = "Original Title", Subtitle = "Original Subtitle" };
        _context.Dashboards.Add(dashboard);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateDashboardDto
        {
            Title = "Updated Title",
            Subtitle = "Updated Subtitle"
        };

        var result = await _controller.UpdateDashboard(dashboard.Id, updateDto);

        Assert.IsType<OkResult>(result);

        var updatedDashboard = await _context.Dashboards.FindAsync(dashboard.Id);
        Assert.NotNull(updatedDashboard);
        Assert.Equal("Updated Title", updatedDashboard.Title);
        Assert.Equal("Updated Subtitle", updatedDashboard.Subtitle);
    }

    [Fact]
    public async Task UpdateDashboard_WithInvalidId_ReturnsNotFound()
    {
        var updateDto = new UpdateDashboardDto
        {
            Title = "Updated Title",
            Subtitle = "Updated Subtitle"
        };

        var result = await _controller.UpdateDashboard(999, updateDto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task SeedData_WithEmptyDatabase_CreatesDefaultDashboard()
    {
        var result = await _controller.SeedData();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Database seeded with sample dashboard.", okResult.Value);

        var dashboard = await _context.Dashboards.FirstOrDefaultAsync();
        Assert.NotNull(dashboard);
        Assert.Equal("Dashy.Net Home Lab", dashboard.Title);
    }

    [Fact]
    public async Task SeedData_WithExistingData_ReturnsOkWithMessage()
    {
        var existingDashboard = new Dashboard { Title = "Existing Dashboard" };
        _context.Dashboards.Add(existingDashboard);
        await _context.SaveChangesAsync();

        var result = await _controller.SeedData();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Database already has data.", okResult.Value);

        var dashboardCount = await _context.Dashboards.CountAsync();
        Assert.Equal(1, dashboardCount);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
