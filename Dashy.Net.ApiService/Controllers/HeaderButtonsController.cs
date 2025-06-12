using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Dashy.Net.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/header-buttons")]
public class HeaderButtonsController(AppDbContext dbContext, ILogger<HeaderButtonsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateHeaderButton([FromBody] CreateHeaderButtonDto buttonDto)
    {
        var dashboard = await dbContext.Dashboards.FindAsync(buttonDto.DashboardId);
        if (dashboard is null)
        {
            return BadRequest(new { message = $"Dashboard with ID {buttonDto.DashboardId} not found." });
        }

        var position = await dbContext.HeaderButtons.CountAsync(b => b.DashboardId == buttonDto.DashboardId);

        var newButton = new HeaderButton
        {
            Text = buttonDto.Text,
            Url = buttonDto.Url,
            Icon = buttonDto.Icon,
            Position = position,
            DashboardId = buttonDto.DashboardId,
            Dashboard = null
        };

        dbContext.HeaderButtons.Add(newButton);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Created new header button '{ButtonText}'", newButton.Text);
        var buttonVm = new HeaderButtonVm(
            newButton.Id,
            newButton.Text,
            newButton.Url,
            newButton.Icon
        );

        return StatusCode(201, buttonVm);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHeaderButton(int id, [FromBody] UpdateHeaderButtonDto buttonDto)
    {
        var button = await dbContext.HeaderButtons.FindAsync(id);
        if (button is null)
        {
            return NotFound();
        }

        button.Text = buttonDto.Text;
        button.Url = buttonDto.Url;
        button.Icon = buttonDto.Icon;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Updated header button with ID {ButtonId}", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHeaderButton(int id)
    {
        var button = await dbContext.HeaderButtons.FindAsync(id);
        if (button is null)
        {
            return NotFound();
        }

        dbContext.HeaderButtons.Remove(button);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Deleted header button with ID {ButtonId}", id);
        return NoContent();
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> ReorderHeaderButtons([FromBody] ReorderHeaderButtonsDto dto)
    {
        var buttonsToReorder = await dbContext.HeaderButtons
            .Where(b => dto.OrderedButtonIds.Contains(b.Id))
            .ToListAsync();

        for (int i = 0; i < dto.OrderedButtonIds.Count; i++)
        {
            var buttonId = dto.OrderedButtonIds[i];
            var button = buttonsToReorder.FirstOrDefault(b => b.Id == buttonId);
            if (button != null)
            {
                button.Position = i;
            }
        }

        await dbContext.SaveChangesAsync();
        return Ok();
    }
}