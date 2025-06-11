using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Dashy.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Text.Json;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(AppDbContext dbContext, ILogger<DashboardController> logger) : ControllerBase
{
    /// <summary>
    /// Gets the entire configuration for the dashboard view.
    /// This is the primary endpoint for the frontend to load its state.
    /// </summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetConfig()
    {

        var dashboard = await dbContext.Dashboards
            .Include(d => d.Sections.OrderBy(s => s.Position))
            .ThenInclude(s => s.Items.OrderBy(i => i.Position))
            .Include(d => d.HeaderButtons.OrderBy(b => b.Position))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (dashboard is null)
        {
            // If no dashboards exist, return an empty one.
            return Ok(new DashboardConfigVm(0,"No Dashboard Found","N/A",new(),new()));
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
                    dbItem.Title!,
                    dbItem.Description,
                    dbItem.Url,
                    dbItem.Icon,
                    dbItem.Widget,
                    dbItem.SectionId,
                    string.IsNullOrWhiteSpace(dbItem.OptionsJson) ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(dbItem.OptionsJson)
                )).ToList()
            )).ToList(),
            dashboard.HeaderButtons.Select(dbButton => new HeaderButtonVm(
                dbButton.Id,
                dbButton.Text,
                dbButton.Url,
                dbButton.Icon
            )).ToList()
        );

        return Ok(configVm);
    }

    /// <summary>
    /// A temporary developer utility to seed an empty database with sample data.
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> SeedData()
    {
        try
        {
            // The check is now on the top-level Dashboards table.
            if (await dbContext.Dashboards.AnyAsync())
            {
                return Ok("Database already has data.");
            }

            logger.LogInformation("No data found. Seeding new sample dashboard...");

            var dashboard = new Dashboard
            {
                Title = "Dashy.Net Home Lab",
                Subtitle = "Your new dashboard, ready to go!",
                HeaderButtons =
                [
                    new HeaderButton 
                    { 
                        Text = "GitHub", 
                        Url = "https://github.com/Lissy93/dashy", 
                        Position = 0,
                        Dashboard = null
                    },
                    new HeaderButton 
                    { 
                        Text = "Documentation", 
                        Url = "https://dashy.to/docs", 
                        Position = 1, 
                        Dashboard = null
                    }
                ],
                Sections =
                [
                    // Create the first section
                    new DashboardSection
                    {
                        Name = "Networking",
                        Icon = "fas fa-network-wired",
                        Position = 0,
                        Dashboard = new Dashboard{
                            Title = "Dashy.Net Home Lab",
                            Subtitle = "Your new dashboard, ready to go!"
                        },
                        Items =
                        [
                            new DashboardItem { Title = "Router", Widget = "static-link", Url = "#", Icon = "fas fa-road-bridge", Position = 0 },
                            new DashboardItem { Title = "Pi-hole", Widget = "static-link", Url = "#", Icon = "fas fa-shield-alt", Position = 1 }
                        ]
                    },
                    new DashboardSection
                    {
                        Name = "Networking",
                        Icon = "fas fa-network-wired",
                        Position = 1,
                        Dashboard = new Dashboard{
                            Title = "Dashy.Net Home Lab",
                            Subtitle = "Your new dashboard, ready to go!"
                        },
                        Items =
                        [
                            new DashboardItem { Title = "Router", Widget = "static-link", Url = "#", Icon = "fas fa-road-bridge", Position = 0 },
                            new DashboardItem { Title = "Pi-hole", Widget = "static-link", Url = "#", Icon = "fas fa-shield-alt", Position = 1 }
                        ]
                    },
                ]
            };
            dbContext.Dashboards.Add(dashboard);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Database was successfully seeded with a sample dashboard.");
            return Ok("Database seeded with sample dashboard.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            return Problem("An error occurred while seeding the database.", statusCode: 500);
        }
    }
}