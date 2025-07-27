using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Dashy.Net.ApiService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dashy.Net.Tests.Unit.Controllers;

public class SectionsControllerTests
{
    private readonly Mock<ILogger<SectionsController>> _mockLogger;
    private readonly AppDbContext _context;

    public SectionsControllerTests()
    {
        _mockLogger = new Mock<ILogger<SectionsController>>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task CreateSection_WithValidDto_CreatesSuccessfully()
    {
        var dashboard = new Dashboard { Title = "Test Dashboard" };
        _context.Dashboards.Add(dashboard);
        await _context.SaveChangesAsync();

        var createDto = new CreateSectionDto
        {
            Name = "Test Section",
            DashboardId = dashboard.Id
        };

        var controller = new SectionsController(_context, _mockLogger.Object);

        var result = await controller.CreateSection(createDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var sectionVm = Assert.IsType<SectionVm>(createdResult.Value);
        Assert.Equal("Test Section", sectionVm.Name);
    }

    [Fact]
    public async Task GetSection_WithValidId_ReturnsSection()
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

        var controller = new SectionsController(_context, _mockLogger.Object);

        var result = await controller.GetSection(section.Id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSection = Assert.IsType<DashboardSection>(okResult.Value);
        Assert.Equal("Test Section", returnedSection.Name);
    }

    [Fact]
    public async Task UpdateSection_WithValidData_UpdatesSuccessfully()
    {
        var dashboard = new Dashboard { Title = "Test Dashboard" };
        _context.Dashboards.Add(dashboard);
        await _context.SaveChangesAsync();

        var section = new DashboardSection 
        { 
            Name = "Original Name", 
            DashboardId = dashboard.Id,
            Dashboard = dashboard
        };
        _context.Sections.Add(section);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateSectionDto
        {
            Name = "Updated Name"
        };

        var controller = new SectionsController(_context, _mockLogger.Object);

        var result = await controller.UpdateSection(section.Id, updateDto);

        Assert.IsType<NoContentResult>(result);
        
        var updatedSection = await _context.Sections.FindAsync(section.Id);
        Assert.NotNull(updatedSection);
        Assert.Equal("Updated Name", updatedSection.Name);
    }

    [Fact]
    public async Task DeleteSection_WithValidId_DeletesSuccessfully()
    {
        var dashboard = new Dashboard { Title = "Test Dashboard" };
        _context.Dashboards.Add(dashboard);
        await _context.SaveChangesAsync();

        var section = new DashboardSection 
        { 
            Name = "Section to Delete", 
            DashboardId = dashboard.Id,
            Dashboard = dashboard
        };
        _context.Sections.Add(section);
        await _context.SaveChangesAsync();

        var controller = new SectionsController(_context, _mockLogger.Object);

        var result = await controller.DeleteSection(section.Id);

        Assert.IsType<NoContentResult>(result);
        
        var deletedSection = await _context.Sections.FindAsync(section.Id);
        Assert.Null(deletedSection);
    }

    [Fact]
    public async Task ReorderSections_WithValidDto_ReordersSuccessfully()
    {
        var dashboard = new Dashboard { Title = "Test Dashboard" };
        _context.Dashboards.Add(dashboard);
        await _context.SaveChangesAsync();

        var sections = new[]
        {
            new DashboardSection { Name = "Section 1", DashboardId = dashboard.Id, Dashboard = dashboard },
            new DashboardSection { Name = "Section 2", DashboardId = dashboard.Id, Dashboard = dashboard },
            new DashboardSection { Name = "Section 3", DashboardId = dashboard.Id, Dashboard = dashboard }
        };

        _context.Sections.AddRange(sections);
        await _context.SaveChangesAsync();

        var reorderDto = new ReorderSectionsDto(new List<int> { sections[2].Id, sections[0].Id, sections[1].Id });

        var controller = new SectionsController(_context, _mockLogger.Object);

        var result = await controller.ReorderSections(reorderDto);

        Assert.IsType<OkResult>(result);
        
        var reorderedSections = await _context.Sections.OrderBy(s => s.Position).ToListAsync();
        Assert.Equal("Section 3", reorderedSections[0].Name);
        Assert.Equal("Section 1", reorderedSections[1].Name);
        Assert.Equal("Section 2", reorderedSections[2].Name);
    }
}
