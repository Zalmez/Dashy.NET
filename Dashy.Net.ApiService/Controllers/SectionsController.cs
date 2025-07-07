﻿using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/sections")]
[Produces("application/json")]
public class SectionsController(AppDbContext dbContext, ILogger<SectionsController> logger) : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionDto sectionDto)
    {
        var dashboard = await dbContext.Dashboards.FindAsync(sectionDto.DashboardId);
        if (dashboard is null)
        {
            return BadRequest(new { message = $"Cannot create section in non-existent Dashboard ID {sectionDto.DashboardId}." });
        }

        var newSection = new DashboardSection
        {
            Dashboard = dashboard,
            Name = sectionDto.Name,
            Icon = sectionDto.Icon,
            DashboardId = sectionDto.DashboardId,
            Position = await dbContext.Sections.CountAsync(s => s.DashboardId == sectionDto.DashboardId)
        };

        dbContext.Sections.Add(newSection);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Created new section '{SectionName}' in Dashboard {DashboardId}", newSection.Name, newSection.DashboardId);

        var sectionVm = new SectionVm(newSection.Id, newSection.Name, newSection.Icon, newSection.DashboardId, new List<ItemVm>());

        return CreatedAtAction(nameof(GetSection), new { id = newSection.Id }, sectionVm);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSection(int id)
    {
        var section = await dbContext.Sections
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (section is null)
        {
            return NotFound();
        }
        return Ok(section);
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSection(int id, [FromBody] UpdateSectionDto sectionDto)
    {
        var sectionToUpdate = await dbContext.Sections.FindAsync(id);
        if (sectionToUpdate is null)
        {
            logger.LogWarning("Attempted to update non-existent section with ID: {SectionId}", id);
            return NotFound();
        }

        sectionToUpdate.Name = sectionDto.Name;
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Updated section with ID: {SectionId}", id);
        return NoContent();
    }

    [HttpPost("reorder")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderSections([FromBody] ReorderSectionsDto dto)
    {
        var sectionsToReorder = await dbContext.Sections
            .Where(section => dto.OrderedSectionIds.Contains(section.Id))
            .ToListAsync();

        for (int i = 0; i < dto.OrderedSectionIds.Count; i++)
        {
            var sectionId = dto.OrderedSectionIds[i];
            var section = sectionsToReorder.FirstOrDefault(s => s.Id == sectionId);
            if (section != null)
            {
                section.Position = i;
            }
        }

        await dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSection(int id)
    {
        var section = await dbContext.Sections
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (section is null)
        {
            logger.LogWarning("Attempted to delete non-existent section with ID: {SectionId}", id);
            return NotFound();
        }
        dbContext.Sections.Remove(section);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Successfully deleted section '{SectionName}' with ID: {SectionId}", section.Name, id);
        return NoContent();
    }
}