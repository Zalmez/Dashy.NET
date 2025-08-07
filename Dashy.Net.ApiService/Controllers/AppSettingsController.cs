using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppSettingsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AppSettingsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<AppSettings>> Get()
    {
        var settings = await _dbContext.AppSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new AppSettings();
            _dbContext.AppSettings.Add(settings);
            await _dbContext.SaveChangesAsync();
        }
        return Ok(settings);
    }

    [HttpPost("logo-path")]
    public async Task<ActionResult> SetLogoPath([FromBody] SetLogoPathRequest request)
    {
        var settings = await _dbContext.AppSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new AppSettings();
            _dbContext.AppSettings.Add(settings);
        }
        settings.CustomLogoPath = request.Path;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("logo")]
    public async Task<ActionResult> RemoveLogo()
    {
        var settings = await _dbContext.AppSettings.FirstOrDefaultAsync();
        if (settings == null || string.IsNullOrEmpty(settings.CustomLogoPath))
            return NotFound();
        settings.CustomLogoPath = null;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}

public class SetLogoPathRequest
{
    public string Path { get; set; } = string.Empty;
}
