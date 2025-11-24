using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
[Authorize]
public class DashboardController(AppDbContext dbContext, ILogger<DashboardController> logger) : ControllerBase
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static string ComputeConfigETag(Dashboard dashboard)
        => $"W/\"{dashboard.LastModifiedUtc.Ticks}\"";

    /// <summary>
    /// Gets the entire configuration for the dashboard view, including sections and items.
    /// This is the primary endpoint for the frontend to load its initial state.
    /// </summary>
    /// <param name="id">Optional dashboard ID. If not specified, returns the first dashboard.</param>
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfig([FromQuery] int? id = null)
    {
        Dashboard? dashboard = id.HasValue
            ? await dbContext.Dashboards
                .Include(d => d.Sections.OrderBy(s => s.Position)).ThenInclude(s => s.Items.OrderBy(i => i.Position))
                .Include(d => d.HeaderButtons.OrderBy(b => b.Position))
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id.Value)
            : await dbContext.Dashboards
                .Include(d => d.Sections.OrderBy(s => s.Position)).ThenInclude(s => s.Items.OrderBy(i => i.Position))
                .Include(d => d.HeaderButtons.OrderBy(b => b.Position))
                .AsNoTracking()
                .FirstOrDefaultAsync();

        if (dashboard is null)
        {
            if (id.HasValue)
            {
                logger.LogWarning("Dashboard with ID {DashboardId} not found.", id.Value);
                return NotFound(new DashboardConfigVm(0, "Dashboard Not Found", $"Dashboard with ID {id.Value} was not found.", [], [], false));
            }
            logger.LogWarning("No dashboard found in the database. Seeding might be required.");
            return NotFound(new DashboardConfigVm(0, "No Dashboard Found", "Please seed the database.", [], [], false));
        }

        var etag = ComputeConfigETag(dashboard);
        if (Request.Headers.IfNoneMatch.Contains(etag))
        {
            logger.LogDebug("Dashboard config not modified (ETag {ETag}). Returning 304.", etag);
            return StatusCode(StatusCodes.Status304NotModified);
        }

        var configVm = new DashboardConfigVm(
            dashboard.Id,
            dashboard.Title,
            dashboard.Subtitle,
            dashboard.Sections.Select(dbSection => new SectionVm(
                dbSection.Id,
                dbSection.Name,
                dbSection.Icon,
                dbSection.DashboardId,
                dbSection.Items.Select(dbItem => new ItemVm(
                    dbItem.Id,
                    dbItem.Title ?? "Untitled Item",
                    dbItem.Icon,
                    dbItem.Widget,
                    dbItem.SectionId,
                    string.IsNullOrWhiteSpace(dbItem.OptionsJson) ? (JsonElement?)null : JsonDocument.Parse(dbItem.OptionsJson).RootElement.Clone(),
                    dbItem.ParentItemId
                )).ToList()
            )).ToList(),
            dashboard.HeaderButtons.Select(dbButton => new HeaderButtonVm(
                dbButton.Id,
                dbButton.Text,
                dbButton.Url,
                dbButton.Icon
            )).ToList(),
            dashboard.UseContainerWidgets
        );

        Response.Headers.ETag = etag;
        return Ok(configVm);
    }

    /// <summary>
    /// Gets a list of all dashboards (id, title, subtitle only).
    /// This is used for dropdown navigation.
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardList()
    {
        var dashboards = await dbContext.Dashboards
            .Select(d => new DashboardListItemVm(d.Id, d.Title, d.Subtitle))
            .AsNoTracking()
            .ToListAsync();
        return Ok(dashboards);
    }

    /// <summary>
    /// Creates a new dashboard.
    /// </summary>
    /// <param name="dashboardDto">The details of the dashboard to create.</param>
    /// <returns>The created dashboard configuration.</returns>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateDashboard([FromBody] CreateDashboardDto dashboardDto)
    {
        try
        {
            var newDashboard = new Dashboard
            {
                Title = dashboardDto.Title,
                Subtitle = dashboardDto.Subtitle,
                LastModifiedUtc = DateTime.UtcNow
            };
            dbContext.Dashboards.Add(newDashboard);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Created new dashboard '{DashboardTitle}' with ID {DashboardId}", newDashboard.Title, newDashboard.Id);
            var configVm = new DashboardConfigVm(newDashboard.Id, newDashboard.Title, newDashboard.Subtitle, [], [], false);
            Response.Headers.ETag = ComputeConfigETag(newDashboard);
            return CreatedAtAction(nameof(GetConfig), new { id = newDashboard.Id }, configVm);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create dashboard");
            return Problem("Failed to create dashboard", statusCode: 500);
        }
    }

    /// <summary>
    /// Updates the dashboard's basic settings like title and subtitle.
    /// </summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateDashboard(int id, [FromBody] UpdateDashboardDto updateDto)
    {
        var dashboard = await dbContext.Dashboards
            .Include(d => d.Sections)
            .Include(d => d.HeaderButtons)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (dashboard is null) return NotFound();

        // Concurrency: validate If-Match when provided
        var currentEtag = ComputeConfigETag(dashboard);
        if (Request.Headers.IfMatch.Count > 0 && !Request.Headers.IfMatch.Contains(currentEtag))
        {
            logger.LogWarning("ETag mismatch on update for dashboard {DashboardId}. Provided: {Provided}, Current: {Current}", id, string.Join(',', Request.Headers.IfMatch), currentEtag);
            return StatusCode(StatusCodes.Status412PreconditionFailed, new { message = "Dashboard has changed. Refresh and retry." });
        }

        dashboard.Title = updateDto.Title;
        dashboard.Subtitle = updateDto.Subtitle;
        dashboard.LastModifiedUtc = DateTime.UtcNow;

        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Dashboard {DashboardId} updated successfully", id);
            Response.Headers.ETag = ComputeConfigETag(dashboard);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update dashboard {DashboardId}", id);
            return Problem("Failed to update dashboard", statusCode: 500);
        }
    }

    /// <summary>
    /// A temporary developer utility to seed an empty database with sample data.
    /// </summary>
    [HttpPost("seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SeedData()
    {
        try
        {
            if (await dbContext.Dashboards.AnyAsync())
            {
                return Ok("Database already has data.");
            }
            logger.LogInformation("No data found. Seeding new sample dashboard...");
            var dashboard = new Dashboard
            {
                Title = "Dashy.Net Home Lab",
                Subtitle = "Your new dashboard, ready to go!",
                LastModifiedUtc = DateTime.UtcNow,
                HeaderButtons =
                [
                    new HeaderButton { Text = "GitHub", Url = "https://github.com/Lissy93/dashy", Position = 0 },
                    new HeaderButton { Text = "Documentation", Url = "https://dashy.to/docs", Position = 1 }
                ],
                Sections =
                [
                    new DashboardSection
                    {
                        Name = "Networking",
                        Icon = "fas fa-network-wired",
                        Position = 0,
                        LastModifiedUtc = DateTime.UtcNow,
                        Items =
                        [
                            new DashboardItem { Title = "Router", Widget = "static-link", Url = "#", Icon = "fas fa-road-bridge", Position = 0, LastModifiedUtc = DateTime.UtcNow },
                            new DashboardItem { Title = "Pi-hole", Widget = "static-link", Url = "#", Icon = "fas fa-shield-alt", Position = 1, LastModifiedUtc = DateTime.UtcNow }
                        ]
                    },
                    new DashboardSection
                    {
                        Name = "Media Servers",
                        Icon = "fas fa-photo-film",
                        Position = 1,
                        LastModifiedUtc = DateTime.UtcNow,
                        Items =
                        [
                            new DashboardItem { Title = "Jellyfin", Widget = "static-link", Url = "#", Icon = "fas fa-tv", Position = 0, LastModifiedUtc = DateTime.UtcNow }
                        ]
                    }
                ]
            };
            dbContext.Dashboards.Add(dashboard);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Database was successfully seeded with a sample dashboard.");
            Response.Headers.ETag = ComputeConfigETag(dashboard);
            return Ok("Database seeded with sample dashboard.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            return Problem("An error occurred while seeding the database.", statusCode: 500);
        }
    }

    /// <summary>
    /// Sets the dashboard's UseContainerWidgets flag.
    /// </summary>
    [HttpPost("{id:int}/use-container-widgets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetUseContainerWidgets(int id, [FromBody] ToggleContainerWidgetsDto dto)
    {
        var dashboard = await dbContext.Dashboards
            .Include(d => d.Sections)
            .Include(d => d.HeaderButtons)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (dashboard is null) return NotFound();
        dashboard.UseContainerWidgets = dto.Enabled;
        dashboard.LastModifiedUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Dashboard {DashboardId} UseContainerWidgets set to {Enabled}", id, dto.Enabled);
        Response.Headers.ETag = ComputeConfigETag(dashboard);
        return Ok(new { id = dashboard.Id, enabled = dashboard.UseContainerWidgets });
    }

    /// <summary>
    /// Gets a flattened list of items for the dashboard, including root items and children of container widgets.
    /// </summary>
    /// <param name="id">The ID of the dashboard.</param>
    [HttpGet("{id:int}/items/flat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFlattenedItems(int id)
    {
        var dashboard = await dbContext.Dashboards
            .Include(d => d.Sections)
            .ThenInclude(s => s.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
        if (dashboard is null) return NotFound();

        var rootItems = dashboard.Sections
            .SelectMany(s => s.Items)
            .Where(i => i.ParentItemId == null)
            .OrderBy(i => i.Position)
            .Select(i => new ItemVm(
                i.Id,
                i.Title ?? "Untitled Item",
                i.Icon,
                i.Widget,
                i.SectionId,
                string.IsNullOrWhiteSpace(i.OptionsJson) ? (JsonElement?)null : JsonDocument.Parse(i.OptionsJson).RootElement.Clone(),
                i.ParentItemId
            )).ToList();

        var childrenLookup = dashboard.Sections
            .SelectMany(s => s.Items)
            .Where(i => i.ParentItemId != null)
            .GroupBy(i => i.ParentItemId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(ci => ci.Position).Select(ci => new ItemVm(
                ci.Id,
                ci.Title ?? "Untitled Item",
                ci.Icon,
                ci.Widget,
                ci.SectionId,
                string.IsNullOrWhiteSpace(ci.OptionsJson) ? (JsonElement?)null : JsonDocument.Parse(ci.OptionsJson).RootElement.Clone(),
                ci.ParentItemId
            )).ToList());

        Response.Headers.ETag = ComputeConfigETag(dashboard);
        return Ok(new { rootItems, children = childrenLookup });
    }
}

public class ToggleContainerWidgetsDto
{
    public bool Enabled { get; set; }
}