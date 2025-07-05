using Dashy.Net.ApiService.Services;
using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Dashy.Net.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/auth-settings")]
public class AuthenticationSettingsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly AuthenticationSettingsProvider _settingsProvider;
    private readonly OneTimeTokenService _oneTimeTokenService;
    private readonly ILogger<AuthenticationSettingsController> _logger;
    
    public AuthenticationSettingsController(
        AppDbContext dbContext, 
        AuthenticationSettingsProvider settingsProvider,
        OneTimeTokenService oneTimeTokenService,
        ILogger<AuthenticationSettingsController> logger)
    {
        _dbContext = dbContext;
        _settingsProvider = settingsProvider;
        _oneTimeTokenService = oneTimeTokenService;
        _logger = logger;
    }

    private async Task<bool> IsAuthEnabledAsync()
    {
        var settings = await _dbContext.AuthenticationSettings.AsNoTracking().FirstOrDefaultAsync();
        return settings != null && settings.IsEnabled;
    }

    private bool IsAuthorizedForAdmin()
    {
        // Check if one-time token is provided and valid
        var oneTimeToken = Request.Query["token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(oneTimeToken) && _oneTimeTokenService.ValidateAndConsumeToken(oneTimeToken))
        {
            _logger.LogInformation("Access granted using one-time token. Token has been consumed and regenerated.");
            return true;
        }

        // If no valid one-time token, check normal authentication
        return User.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    /// Gets the current authentication settings. 
    /// Available to everyone if authentication is disabled, or to authorized users if enabled.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AuthenticationSettings>> Get()
    {
        var settings = await _dbContext.AuthenticationSettings.AsNoTracking().FirstOrDefaultAsync();
        
        // If no settings exist, return default empty settings
        if (settings == null)
        {
            return Ok(new AuthenticationSettings());
        }

        // If authentication is enabled, check authorization (including one-time token)
        if (settings.IsEnabled)
        {
            if (!IsAuthorizedForAdmin())
                return Forbid();
        }

        // Never return the actual client secret
        settings.ClientSecret = null;
        return Ok(settings);
    }

    /// <summary>
    /// Updates the authentication settings. 
    /// Available to everyone if authentication is disabled, or to authorized users if enabled.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AuthenticationSettings updated)
    {
        var isAuthEnabled = await IsAuthEnabledAsync();
        
        // If authentication is currently enabled, require authorization (including one-time token)
        if (isAuthEnabled && !IsAuthorizedForAdmin())
            return Forbid();

        var existing = await _dbContext.AuthenticationSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            // Encrypt secret if provided
            if (!string.IsNullOrWhiteSpace(updated.ClientSecret))
                updated.ClientSecret = AesEncryptionService.Encrypt(updated.ClientSecret);
            
            _dbContext.AuthenticationSettings.Add(updated);
            _logger.LogInformation("Authentication settings created");
        }
        else
        {
            existing.IsEnabled = updated.IsEnabled;
            existing.Authority = updated.Authority;
            existing.ClientId = updated.ClientId;
            existing.TenantId = updated.TenantId;
            
            // Only update client secret if provided
            if (!string.IsNullOrWhiteSpace(updated.ClientSecret))
                existing.ClientSecret = AesEncryptionService.Encrypt(updated.ClientSecret);
                
            _logger.LogInformation("Authentication settings updated");
        }
        
        await _dbContext.SaveChangesAsync();
        
        // Clear cache so next request gets fresh settings
        _settingsProvider.ClearCache();
        
        return NoContent();
    }

    /// <summary>
    /// Gets the current one-time token for authentication configuration.
    /// This endpoint is always accessible and provides the token needed for secure access.
    /// </summary>
    [HttpGet("token")]
    public ActionResult<object> GetOneTimeToken()
    {
        var token = _oneTimeTokenService.GetCurrentToken();
        var expiresAt = _oneTimeTokenService.GetTokenExpiration();
        
        return Ok(new
        {
            Token = token,
            ExpiresAt = expiresAt,
            Message = "Use this token as a query parameter (?token=...) to access authentication settings. Token will be regenerated after use.",
            Example = $"/api/auth-settings?token={token}"
        });
    }
}
