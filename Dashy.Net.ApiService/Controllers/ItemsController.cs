using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Dashy.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/items")]
public class ItemsController(AppDbContext dbContext, ILogger<ItemsController> logger) : ControllerBase
{
    [HttpPost]
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
            OptionsJson = itemDto.Options is not null ? JsonSerializer.Serialize(itemDto.Options) : null
        };

        dbContext.Items.Add(newItem);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Created new item '{ItemTitle}' with ID {ItemId} in section {SectionId}", newItem.Title, newItem.Id, newItem.SectionId);

        // Map the new entity to the clean ViewModel for the response.
        var itemVm = new ItemVm(
            newItem.Id,
            newItem.Title,
            newItem.Description,
            newItem.Url,
            newItem.Icon,
            newItem.Widget,
            newItem.SectionId,
            itemDto.Options
        );

        return CreatedAtAction(nameof(GetItem), new { id = newItem.Id }, itemVm);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetItem(int id)
    {
        var item = await dbContext.Items.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        // Map the entity to the clean ViewModel before returning.
        var itemVm = new ItemVm(
            item.Id,
            item.Title!,
            item.Description,
            item.Url,
            item.Icon,
            item.Widget,
            item.SectionId,
            string.IsNullOrWhiteSpace(item.OptionsJson) ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(item.OptionsJson)
        );

        return Ok(itemVm);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateItemDto itemDto)
    {
        var itemToUpdate = await dbContext.Items.FindAsync(id);
        if (itemToUpdate is null)
        {
            logger.LogWarning("Attempted to update non-existent item with ID: {ItemId}", id);
            return NotFound();
        }

        itemToUpdate.Title = itemDto.Title;
        itemToUpdate.Description = itemDto.Description;
        itemToUpdate.Url = itemDto.Url;
        itemToUpdate.Icon = itemDto.Icon;
        itemToUpdate.Widget = itemDto.Widget;
        itemToUpdate.SectionId = itemDto.SectionId;
        itemToUpdate.OptionsJson = itemDto.Options is not null ? JsonSerializer.Serialize(itemDto.Options) : null;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Updated item with ID: {ItemId}", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
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
}