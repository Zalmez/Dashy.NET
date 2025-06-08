using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.DTOs;
using Dashy.Net.Shared.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Dashy.Net.ApiService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController(AppDbContext dbContext, ILogger<DashboardController> logger) : ControllerBase
{

    #region getters

    [HttpGet("items/{id}")]
    public async Task<IActionResult> GetItem(int id)
    {
        var item = await dbContext.Items.FindAsync(id);

        if (item is null)
        {
            return NotFound();
        }

        // In a real app, we would map this to a ViewModel before returning.
        return Ok(item);
    }

    [HttpGet("config")]
    public async Task<IActionResult> GetConfig()
    {
        var sectionsFromDb = await dbContext.Sections
            .Include(s => s.Items)
            .AsNoTracking()
            .ToListAsync();

        // --- Map the Database Entities to the ViewModels ---
        var configVm = new DashboardConfigVm(
            "Database-Driven Dashboard",
            sectionsFromDb.Select(dbSection => new SectionVm
            (
                dbSection.Id,
                dbSection.Name,
                dbSection.Items.Select(dbItem => new ItemVm
                (
                    dbItem.Id,
                    dbItem.Title,
                    dbItem.Description,
                    dbItem.Url,
                    dbItem.Icon,
                    dbItem.Widget,
                    dbItem.SectionId,
                    string.IsNullOrWhiteSpace(dbItem.OptionsJson)
                        ? null
                        : JsonSerializer.Deserialize<Dictionary<string, object>>(dbItem.OptionsJson)
                )).ToList()
            )).ToList()
        );

        return Ok(configVm);
    }

    #endregion

    #region Posts

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
                // 2. We create the child 'items' and put them INSIDE the parent's 'Items' list
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

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto itemDto)
    {
        var section = await dbContext.Sections.FindAsync(itemDto.SectionId);
        if (section is null)
        {
            return BadRequest(new { message = $"Cannot create item in non-existent Section ID {itemDto.SectionId}." });
        }

        var newItem = new DashboardItem
        {
            Title = itemDto.Title,
            Description = itemDto.Description,
            Url = itemDto.Url,
            Icon = itemDto.Icon,
            Widget = itemDto.Widget,
            SectionId = itemDto.SectionId,
            OptionsJson = itemDto.Options is not null
                ? JsonSerializer.Serialize(itemDto.Options)
                : null
        };

        dbContext.Items.Add(newItem);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Created new item '{ItemTitle}' with ID {ItemId} in section {SectionId}", newItem.Title, newItem.Id, newItem.SectionId);

        // The 'CreatedAtAction' returns a proper 201 Created HTTP response.
        // It requires the name of the "Get" action for this resource.
        return CreatedAtAction(nameof(GetItem), new { id = newItem.Id }, newItem);
    }

    #endregion

    #region Deletes
    [HttpDelete("items/{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var item = await dbContext.Items.FindAsync(id);

        if (item is null)
        {
            logger.LogWarning("Attempted to delete non-existent item with ID: {ItemId}", id);
            return NotFound();
        }

        dbContext.Items.Remove(item);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Successfully deleted item with ID: {ItemId}", id);
        return NoContent();
    }
    #endregion
}