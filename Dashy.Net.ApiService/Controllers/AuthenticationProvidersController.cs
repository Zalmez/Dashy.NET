using Dashy.Net.ApiService.Services;
using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Dashy.Net.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/auth-providers")]
public class AuthenticationProvidersController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly AuthenticationProviderTemplateService _templateService;
    private readonly OneTimeTokenService _oneTimeTokenService;
    private readonly ILogger<AuthenticationProvidersController> _logger;

    public AuthenticationProvidersController(
        AppDbContext dbContext,
        AuthenticationProviderTemplateService templateService,
        OneTimeTokenService oneTimeTokenService,
        ILogger<AuthenticationProvidersController> logger)
    {
        _dbContext = dbContext;
        _templateService = templateService;
        _oneTimeTokenService = oneTimeTokenService;
        _logger = logger;
    }

    private async Task<bool> IsAuthEnabledAsync()
    {
        var authSettings = await _dbContext.AuthenticationSettings.AsNoTracking().FirstOrDefaultAsync();
        return authSettings != null && authSettings.IsEnabled;
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
    /// Gets all authentication provider templates
    /// </summary>
    [HttpGet("templates")]
    public ActionResult<IEnumerable<AuthenticationProviderTemplate>> GetTemplates()
    {
        return Ok(_templateService.GetAllTemplates());
    }

    /// <summary>
    /// Gets all configured authentication providers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthenticationProviderVm>>> GetProviders()
    {
        var isAuthEnabled = await IsAuthEnabledAsync();
        
        // If authentication is enabled, require authorization (including one-time token)
        if (isAuthEnabled && !IsAuthorizedForAdmin())
            return Forbid();

        var providers = await _dbContext.AuthenticationProviders
            .Include(p => p.Settings)
            .OrderBy(p => p.Priority)
            .ToListAsync();

        var result = providers.Select(p => new AuthenticationProviderVm(
            p.Id,
            p.Name,
            p.ProviderType,
            p.IsEnabled,
            p.IsDefault,
            p.Priority,
            p.Settings.ToDictionary(
                s => s.Key,
                s => new AuthenticationProviderSettingVm(
                    s.IsEncrypted && !string.IsNullOrEmpty(s.Value) ? "***" : s.Value,
                    s.IsEncrypted,
                    s.IsRequired
                )
            )
        ));

        return Ok(result);
    }

    /// <summary>
    /// Gets a specific authentication provider by ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthenticationProviderVm>> GetProvider(int id)
    {
        var isAuthEnabled = await IsAuthEnabledAsync();
        
        // If authentication is enabled, require authorization (including one-time token)
        if (isAuthEnabled && !IsAuthorizedForAdmin())
            return Forbid();

        var provider = await _dbContext.AuthenticationProviders
            .Include(p => p.Settings)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (provider == null)
            return NotFound();

        var result = new AuthenticationProviderVm(
            provider.Id,
            provider.Name,
            provider.ProviderType,
            provider.IsEnabled,
            provider.IsDefault,
            provider.Priority,
            provider.Settings.ToDictionary(
                s => s.Key,
                s => new AuthenticationProviderSettingVm(
                    s.IsEncrypted && !string.IsNullOrEmpty(s.Value) ? "***" : s.Value,
                    s.IsEncrypted,
                    s.IsRequired
                )
            )
        );

        return Ok(result);
    }

    /// <summary>
    /// Creates a new authentication provider
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AuthenticationProviderVm>> CreateProvider([FromBody] CreateAuthenticationProviderDto createDto)
    {
        var isAuthEnabled = await IsAuthEnabledAsync();
        
        // If authentication is enabled, require authorization (including one-time token)
        if (isAuthEnabled && !IsAuthorizedForAdmin())
            return Forbid();

        // Check if provider name already exists
        var existingProvider = await _dbContext.AuthenticationProviders
            .FirstOrDefaultAsync(p => p.Name == createDto.Name);

        if (existingProvider != null)
            return Conflict($"Authentication provider with name '{createDto.Name}' already exists");

        var provider = new AuthenticationProvider
        {
            Name = createDto.Name,
            ProviderType = createDto.ProviderType,
            IsEnabled = createDto.IsEnabled,
            IsDefault = createDto.IsDefault,
            Priority = createDto.Priority,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // If this is set as default, unset other defaults
        if (createDto.IsDefault)
        {
            var otherDefaults = await _dbContext.AuthenticationProviders
                .Where(p => p.IsDefault)
                .ToListAsync();
            
            foreach (var otherDefault in otherDefaults)
            {
                otherDefault.IsDefault = false;
            }
        }

        _dbContext.AuthenticationProviders.Add(provider);
        await _dbContext.SaveChangesAsync();

        // Add settings
        foreach (var setting in createDto.Settings)
        {
            var providerSetting = new AuthenticationProviderSettings
            {
                AuthenticationProviderId = provider.Id,
                Key = setting.Key,
                Value = setting.Value.IsEncrypted && !string.IsNullOrEmpty(setting.Value.Value) 
                    ? AesEncryptionService.Encrypt(setting.Value.Value) 
                    : setting.Value.Value,
                IsEncrypted = setting.Value.IsEncrypted,
                IsRequired = setting.Value.IsRequired
            };

            _dbContext.AuthenticationProviderSettings.Add(providerSetting);
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Authentication provider '{Name}' created", provider.Name);

        // Return the created provider
        var result = new AuthenticationProviderVm(
            provider.Id,
            provider.Name,
            provider.ProviderType,
            provider.IsEnabled,
            provider.IsDefault,
            provider.Priority,
            createDto.Settings.ToDictionary(
                s => s.Key,
                s => new AuthenticationProviderSettingVm(
                    s.Value.IsEncrypted && !string.IsNullOrEmpty(s.Value.Value) ? "***" : s.Value.Value,
                    s.Value.IsEncrypted,
                    s.Value.IsRequired
                )
            )
        );

        return CreatedAtAction(nameof(GetProvider), new { id = provider.Id }, result);
    }

    /// <summary>
    /// Updates an existing authentication provider
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateProvider(int id, [FromBody] UpdateAuthenticationProviderDto updateDto)
    {
        var isAuthEnabled = await IsAuthEnabledAsync();
        
        // If authentication is enabled, require authorization (including one-time token)
        if (isAuthEnabled && !IsAuthorizedForAdmin())
            return Forbid();

        var provider = await _dbContext.AuthenticationProviders
            .Include(p => p.Settings)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (provider == null)
            return NotFound();

        // Check if the new name conflicts with another provider
        var existingProvider = await _dbContext.AuthenticationProviders
            .FirstOrDefaultAsync(p => p.Name == updateDto.Name && p.Id != id);

        if (existingProvider != null)
            return Conflict($"Authentication provider with name '{updateDto.Name}' already exists");

        // Update basic properties
        provider.Name = updateDto.Name;
        provider.IsEnabled = updateDto.IsEnabled;
        provider.IsDefault = updateDto.IsDefault;
        provider.Priority = updateDto.Priority;
        provider.UpdatedAt = DateTime.UtcNow;

        // If this is set as default, unset other defaults
        if (updateDto.IsDefault)
        {
            var otherDefaults = await _dbContext.AuthenticationProviders
                .Where(p => p.IsDefault && p.Id != id)
                .ToListAsync();
            
            foreach (var otherDefault in otherDefaults)
            {
                otherDefault.IsDefault = false;
            }
        }

        // Update settings
        foreach (var setting in updateDto.Settings)
        {
            var existingSetting = provider.Settings.FirstOrDefault(s => s.Key == setting.Key);
            
            if (existingSetting != null)
            {
                // Only update if value is provided (not empty for encrypted fields)
                if (!string.IsNullOrEmpty(setting.Value.Value) || !setting.Value.IsEncrypted)
                {
                    existingSetting.Value = setting.Value.IsEncrypted && !string.IsNullOrEmpty(setting.Value.Value)
                        ? AesEncryptionService.Encrypt(setting.Value.Value)
                        : setting.Value.Value;
                }
                existingSetting.IsRequired = setting.Value.IsRequired;
            }
            else
            {
                // Add new setting
                var newSetting = new AuthenticationProviderSettings
                {
                    AuthenticationProviderId = provider.Id,
                    Key = setting.Key,
                    Value = setting.Value.IsEncrypted && !string.IsNullOrEmpty(setting.Value.Value)
                        ? AesEncryptionService.Encrypt(setting.Value.Value)
                        : setting.Value.Value,
                    IsEncrypted = setting.Value.IsEncrypted,
                    IsRequired = setting.Value.IsRequired
                };

                provider.Settings.Add(newSetting);
            }
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Authentication provider '{Name}' updated", provider.Name);

        return NoContent();
    }

    /// <summary>
    /// Deletes an authentication provider
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteProvider(int id)
    {
        var isAuthEnabled = await IsAuthEnabledAsync();
        
        // If authentication is enabled, require authorization (including one-time token)
        if (isAuthEnabled && !IsAuthorizedForAdmin())
            return Forbid();

        var provider = await _dbContext.AuthenticationProviders.FindAsync(id);

        if (provider == null)
            return NotFound();

        _dbContext.AuthenticationProviders.Remove(provider);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Authentication provider '{Name}' deleted", provider.Name);

        return NoContent();
    }
}