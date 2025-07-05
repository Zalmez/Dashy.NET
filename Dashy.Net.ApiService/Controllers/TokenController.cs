using Dashy.Net.ApiService.Services;
using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/token")]
public class TokenController : ControllerBase
{
    private readonly OneTimeTokenService _oneTimeTokenService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TokenController> _logger;

    public TokenController(
        OneTimeTokenService oneTimeTokenService,
        AppDbContext dbContext,
        ILogger<TokenController> logger)
    {
        _oneTimeTokenService = oneTimeTokenService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current one-time token. This endpoint is always accessible.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetToken()
    {
        var token = _oneTimeTokenService.GetCurrentToken();
        var expiresAt = _oneTimeTokenService.GetTokenExpiration();
        
        // Check if authentication is currently enabled
        var authSettings = await _dbContext.AuthenticationSettings.AsNoTracking().FirstOrDefaultAsync();
        var isAuthEnabled = authSettings?.IsEnabled == true;

        var response = new
        {
            Token = token,
            ExpiresAt = expiresAt,
            IsAuthenticationEnabled = isAuthEnabled,
            TimeRemaining = expiresAt - DateTime.UtcNow,
            Usage = new
            {
                Message = "Use this token as a query parameter (?token=...) to access authentication settings",
                AuthSettingsExample = $"{Request.Scheme}://{Request.Host}/api/auth-settings?token={token}",
                ProvidersExample = $"{Request.Scheme}://{Request.Host}/api/auth-providers?token={token}",
                Note = "Token will be consumed and regenerated after each use for security"
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Forces regeneration of the one-time token (for testing/manual refresh)
    /// </summary>
    [HttpPost("regenerate")]
    public ActionResult<object> RegenerateToken()
    {
        var newToken = _oneTimeTokenService.RegenerateToken();
        var expiresAt = _oneTimeTokenService.GetTokenExpiration();
        
        _logger.LogInformation("One-time token manually regenerated via API");

        return Ok(new
        {
            Token = newToken,
            ExpiresAt = expiresAt,
            Message = "New token generated successfully",
            Usage = new
            {
                AuthSettingsExample = $"{Request.Scheme}://{Request.Host}/api/auth-settings?token={newToken}",
                ProvidersExample = $"{Request.Scheme}://{Request.Host}/api/auth-providers?token={newToken}"
            }
        });
    }

    /// <summary>
    /// Gets token information and statistics (for debugging)
    /// </summary>
    [HttpGet("info")]
    public ActionResult<object> GetTokenInfo()
    {
        var tokenInfo = _oneTimeTokenService.GetTokenInfo();
        return Ok(tokenInfo);
    }
}