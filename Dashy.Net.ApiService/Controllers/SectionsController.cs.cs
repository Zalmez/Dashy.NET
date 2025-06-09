using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Dashy.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/sections")]
public class SectionsController(AppDbContext dbContext, ILogger<SectionsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionDto sectionDto)
    {
        var newSection = new DashboardSection
        {
            Name = sectionDto.Name,
            Items = new List<DashboardItem>()
        };

        dbContext.Sections.Add(newSection);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Created new section '{SectionName}' with ID {SectionId}", newSection.Name, newSection.Id);

        // Map to a clean ViewModel for the response. An empty section has an empty list of items.
        var sectionVm = new SectionVm(newSection.Id, newSection.Name, new List<ItemVm>());

        return CreatedAtAction(nameof(GetSection), new { id = newSection.Id }, sectionVm);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSection(int id)
    {
        var section = await dbContext.Sections
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (section is null)
        {
            return NotFound();
        }

        // Map the entity and its children to clean ViewModels
        var sectionVm = new SectionVm(
            section.Id,
            section.Name,
            section.Items.Select(dbItem => new ItemVm(
                dbItem.Id,
                dbItem.Title,
                dbItem.Description,
                dbItem.Url,
                dbItem.Icon,
                dbItem.Widget,
                dbItem.SectionId,
                string.IsNullOrWhiteSpace(dbItem.OptionsJson) ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(dbItem.OptionsJson)
            )).ToList()
        );

        return Ok(sectionVm);
    }
}