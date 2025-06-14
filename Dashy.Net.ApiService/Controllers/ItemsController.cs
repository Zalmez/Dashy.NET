using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/items")]
public class ItemsController(AppDbContext dbContext, ILogger<ItemsController> logger) : ControllerBase
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto itemDto)
    {
        var section = await dbContext.Sections.FindAsync(itemDto.SectionId);
        if (section is null) return BadRequest("Section not found.");

        var newItem = new DashboardItem
        {
            Title = itemDto.Title,
            Icon = itemDto.Icon,
            Widget = itemDto.Widget,
            SectionId = itemDto.SectionId,
            OptionsJson = itemDto.Options is not null ? JsonSerializer.Serialize(itemDto.Options, _jsonOptions) : null
        };

        dbContext.Items.Add(newItem);
        await dbContext.SaveChangesAsync();
        return Ok(MapToVm(newItem));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateItemDto itemDto)
    {
        var item = await dbContext.Items.FindAsync(id);
        if (item is null) return NotFound();

        item.Title = itemDto.Title;
        item.Icon = itemDto.Icon;
        item.Widget = itemDto.Widget;
        item.SectionId = itemDto.SectionId;
        item.OptionsJson = itemDto.Options is not null ? JsonSerializer.Serialize(itemDto.Options, _jsonOptions) : null;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var item = await dbContext.Items.FindAsync(id);
        if (item is null) return NotFound();
        dbContext.Items.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private static ItemVm MapToVm(DashboardItem dbItem)
    {
        var options = string.IsNullOrWhiteSpace(dbItem.OptionsJson)
            ? null
            : JsonSerializer.Deserialize<Dictionary<string, object>>(dbItem.OptionsJson, _jsonOptions);

        return new ItemVm(dbItem.Id, dbItem.Title, dbItem.Icon, dbItem.Widget, dbItem.SectionId, options);
    }
}