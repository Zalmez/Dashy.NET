using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/header-buttons")]
[Produces("application/json")]
public class HeaderButtonsController(AppDbContext dbContext, ILogger<HeaderButtonsController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new header button for a dashboard.
    /// </summary>
    /// <param name="buttonDto">The details of the header button to create.</param>
    /// <returns>The created header button.</returns>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            Dashboard = dashboard
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

    /// <summary>
    /// Updates an existing header button.
    /// </summary>
    /// <param name="id">The ID of the header button to update.</param>
    /// <param name="buttonDto">The updated details of the header button.</param>
    /// <returns>A success message if the update is successful.</returns>
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        return Ok(new { message = "Header button updated successfully." });
    }

    /// <summary>
    /// Deletes a header button by ID.
    /// </summary>
    /// <param name="id">The ID of the header button to delete.</param>
    /// <returns>A success message if the deletion is successful.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        return Ok(new { message = "Header button deleted successfully." });
    }

    /// <summary>
    /// Reorders header buttons for a dashboard.
    /// </summary>
    /// <param name="dto">The new order of header button IDs.</param>
    /// <returns>A success message if the reorder is successful.</returns>
    [HttpPost("reorder")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
        return Ok(new { message = "Header buttons reordered successfully." });
    }
}