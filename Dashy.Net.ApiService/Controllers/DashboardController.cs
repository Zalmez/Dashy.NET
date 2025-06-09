using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Dashy.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        var sectionsFromDb = await dbContext.Sections
            .Include(s => s.Items)
            .AsNoTracking()
            .ToListAsync();

        // Map the Database Entities to the ViewModels the frontend expects
        var configVm = new DashboardConfigVm(
            "Database-Driven Dashboard",
            sectionsFromDb.Select(dbSection => new SectionVm(
                dbSection.Id,
                dbSection.Name,
                dbSection.Items.Select(dbItem => new ItemVm(
                    dbItem.Id,
                    dbItem.Title,
                    dbItem.Description,
                    dbItem.Url,
                    dbItem.Icon,
                    dbItem.Widget,
                    dbItem.SectionId,
                    string.IsNullOrWhiteSpace(dbItem.OptionsJson) ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(dbItem.OptionsJson)
                )).ToList()
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
            if (await dbContext.Sections.AnyAsync())
            {
                return Ok("Database already has data.");
            }

            var section = new DashboardSection
            {
                Name = "From The Database",
                Items =
                [
                    new DashboardItem { Title = "Test Item 1", Widget = "static-link", Url = "#", Icon = "fas fa-database" },
                    new DashboardItem { Title = "Test Item 2", Widget = "static-link", Url = "#", Icon = "fas fa-server" }
                ]
            };

            dbContext.Sections.Add(section);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Database was seeded with sample data successfully.");
            return Ok("Database seeded with sample data.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            return Problem("An error occurred while seeding the database.", statusCode: 500);
        }
    }
}