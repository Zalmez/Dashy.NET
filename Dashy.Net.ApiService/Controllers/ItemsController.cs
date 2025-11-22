using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/items")]
[Produces("application/json")]
[Authorize]
public class ItemsController(AppDbContext dbContext, ILogger<ItemsController> logger) : ControllerBase
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto itemDto)
    {
        var section = await dbContext.Sections.FindAsync(itemDto.SectionId);
        if (section is null) return BadRequest("Section not found.");

        DashboardItem? parentItem = null;
        if (itemDto.ParentItemId.HasValue)
        {
            parentItem = await dbContext.Items.FindAsync(itemDto.ParentItemId.Value);
            if (parentItem is null)
            {
                return BadRequest($"Parent item {itemDto.ParentItemId.Value} not found.");
            }
        }

        var maxPosition = await dbContext.Items.Where(i => i.SectionId == itemDto.SectionId && i.ParentItemId == itemDto.ParentItemId).MaxAsync(i => (int?)i.Position) ?? -1;

        var newItem = new DashboardItem
        {
            Title = itemDto.Title,
            Icon = itemDto.Icon,
            Widget = itemDto.Widget ?? "static-link",
            SectionId = itemDto.SectionId,
            ParentItemId = itemDto.ParentItemId,
            Position = maxPosition + 1,
            OptionsJson = itemDto.Options is not null ? JsonSerializer.Serialize(itemDto.Options, _jsonOptions) : null
        };

        dbContext.Items.Add(newItem);
        await dbContext.SaveChangesAsync();
        return Ok(MapToVm(newItem));
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateItemDto itemDto)
    {
        var item = await dbContext.Items.FindAsync(id);
        if (item is null) return NotFound();

        if (itemDto.ParentItemId.HasValue && itemDto.ParentItemId.Value == id)
        {
            return BadRequest("Item cannot be its own parent.");
        }

        if (itemDto.ParentItemId.HasValue)
        {
            var parentExists = await dbContext.Items.AnyAsync(i => i.Id == itemDto.ParentItemId.Value);
            if (!parentExists) return BadRequest($"Parent item {itemDto.ParentItemId.Value} not found.");
        }

        item.Title = itemDto.Title;
        item.Icon = itemDto.Icon;
        item.Widget = itemDto.Widget ?? "static-link";
        item.SectionId = itemDto.SectionId;
        item.ParentItemId = itemDto.ParentItemId;
        item.OptionsJson = itemDto.Options is not null ? JsonSerializer.Serialize(itemDto.Options, _jsonOptions) : null;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("reorder")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderItems([FromBody] ReorderItemsDto dto)
    {
        var items = await dbContext.Items.Where(i => dto.OrderedItemIds.Contains(i.Id)).ToListAsync();
        for (int i = 0; i < dto.OrderedItemIds.Count; i++)
        {
            var id = dto.OrderedItemIds[i];
            var itm = items.FirstOrDefault(x => x.Id == id);
            if (itm != null)
            {
                itm.Position = i;
            }
        }
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Reordered {Count} items", dto.OrderedItemIds.Count);
        return Ok(new { success = true });
    }

    [HttpPost("reorder/scoped")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderItemsScoped([FromBody] ReorderItemsScopedDto dto)
    {
        var items = await dbContext.Items
            .Where(i => i.SectionId == dto.SectionId && i.ParentItemId == dto.ParentItemId && dto.OrderedItemIds.Contains(i.Id))
            .OrderBy(i => i.Position)
            .ToListAsync();

        // Ensure we only assign positions within the provided list preserving relative order by dto list
        for (int i = 0; i < dto.OrderedItemIds.Count; i++)
        {
            var id = dto.OrderedItemIds[i];
            var itm = items.FirstOrDefault(x => x.Id == id);
            if (itm != null) itm.Position = i;
        }
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Scoped reorder applied: Section {SectionId}, Parent {ParentItemId}, Count {Count}", dto.SectionId, dto.ParentItemId, dto.OrderedItemIds.Count);
        return Ok(new { success = true });
    }

    [HttpPost("move")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveItem([FromBody] MoveItemDto dto)
    {
        var item = await dbContext.Items.FindAsync(dto.ItemId);
        if (item is null) return NotFound(new { message = $"Item {dto.ItemId} not found" });

        if (dto.NewParentItemId.HasValue && dto.NewParentItemId.Value == dto.ItemId)
        {
            return BadRequest(new { message = "Item cannot be its own parent." });
        }

        if (dto.NewSectionId.HasValue)
        {
            var sectionExists = await dbContext.Sections.AnyAsync(s => s.Id == dto.NewSectionId.Value);
            if (!sectionExists) return BadRequest(new { message = $"Section {dto.NewSectionId.Value} not found." });
            item.SectionId = dto.NewSectionId.Value;
        }

        if (dto.NewParentItemId.HasValue)
        {
            var parentExists = await dbContext.Items.AnyAsync(i => i.Id == dto.NewParentItemId.Value);
            if (!parentExists) return BadRequest(new { message = $"Parent item {dto.NewParentItemId.Value} not found." });
            item.ParentItemId = dto.NewParentItemId.Value;
        }
        else if (dto.ClearParent)
        {
            item.ParentItemId = null;
        }

        if (dto.NewPosition.HasValue)
        {
            item.Position = dto.NewPosition.Value;
        }

        await dbContext.SaveChangesAsync();
        return Ok(new { success = true });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        JsonElement? options = null;
        if (!string.IsNullOrWhiteSpace(dbItem.OptionsJson))
        {
            using var doc = JsonDocument.Parse(dbItem.OptionsJson);
            options = doc.RootElement.Clone();
        }

        return new ItemVm(dbItem.Id, dbItem.Title, dbItem.Icon, dbItem.Widget, dbItem.SectionId, options, dbItem.ParentItemId);
    }
}

public class MoveItemDto
{
    public int ItemId { get; set; }
    public int? NewSectionId { get; set; }
    public int? NewParentItemId { get; set; }
    public bool ClearParent { get; set; } = false;
    public int? NewPosition { get; set; }
}

public class ReorderItemsScopedDto
{
    public int SectionId { get; set; }
    public int? ParentItemId { get; set; }
    public List<int> OrderedItemIds { get; set; } = new();
}