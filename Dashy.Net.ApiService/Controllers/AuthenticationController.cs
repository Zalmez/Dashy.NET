using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Authorization;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AuthenticationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(AppDbContext context, ILogger<AuthenticationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("auth-settings")]
    public async Task<ActionResult<AuthenticationSettings>> GetAuthenticationSettings()
    {
        try
        {
            var settings = await _context.AuthenticationSettings.FirstOrDefaultAsync();
            
            // If no settings exist, return default settings
            if (settings == null)
            {
                settings = new AuthenticationSettings
                {
                    IsEnabled = false,
                    Authority = null,
                    ClientId = null,
                    ClientSecret = null,
                    TenantId = null,
                    Scopes = null
                };
            }

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve authentication settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("auth-settings")]
    public async Task<ActionResult> UpdateAuthenticationSettings([FromBody] AuthenticationSettings settings)
    {
        try
        {
            var existing = await _context.AuthenticationSettings.FirstOrDefaultAsync();
            
            if (existing == null)
            {
                // Create new settings
                _context.AuthenticationSettings.Add(settings);
            }
            else
            {
                // Update existing settings
                existing.IsEnabled = settings.IsEnabled;
                existing.Authority = settings.Authority;
                existing.ClientId = settings.ClientId;
                existing.ClientSecret = settings.ClientSecret;
                existing.TenantId = settings.TenantId;
                existing.Scopes = settings.Scopes;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update authentication settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("auth-providers")]
    public async Task<ActionResult<IEnumerable<AuthenticationProviderVm>>> GetAuthenticationProviders()
    {
        try
        {
            var providers = await _context.AuthenticationProviders
                .OrderBy(p => p.Priority)
                .ToListAsync();

            var providerVms = providers.Select(p => new AuthenticationProviderVm(
                p.Id,
                p.Name,
                p.ProviderType,
                p.IsEnabled,
                p.IsDefault,
                p.Priority,
                new Dictionary<string, AuthenticationProviderSettingVm>() // TODO: Load settings if needed
            ));

            return Ok(providerVms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve authentication providers");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("auth-providers/{id}")]
    public async Task<ActionResult<AuthenticationProviderVm>> GetAuthenticationProvider(int id)
    {
        try
        {
            var provider = await _context.AuthenticationProviders.FindAsync(id);
            
            if (provider == null)
            {
                return NotFound();
            }

            var providerVm = new AuthenticationProviderVm(
                provider.Id,
                provider.Name,
                provider.ProviderType,
                provider.IsEnabled,
                provider.IsDefault,
                provider.Priority,
                new Dictionary<string, AuthenticationProviderSettingVm>() // TODO: Load settings if needed
            );

            return Ok(providerVm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve authentication provider {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("auth-providers/{id}")]
    public async Task<ActionResult> DeleteAuthenticationProvider(int id)
    {
        try
        {
            var provider = await _context.AuthenticationProviders.FindAsync(id);
            
            if (provider == null)
            {
                return NotFound();
            }

            _context.AuthenticationProviders.Remove(provider);
            await _context.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete authentication provider {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("auth-providers/templates")]
    public ActionResult<IEnumerable<AuthenticationProviderTemplate>> GetAuthenticationProviderTemplates()
    {
        try
        {
            // For now, return some basic templates
            // This could be moved to a service or configuration file later
            var templates = new List<AuthenticationProviderTemplate>
            {
                new AuthenticationProviderTemplate
                {
                    Name = "Microsoft Entra ID (Azure AD)",
                    ProviderType = "AzureAD",
                    Description = "Authenticate using Microsoft Entra ID (formerly Azure Active Directory)",
                    Settings = new Dictionary<string, AuthenticationProviderSettingTemplate>
                    {
                        ["Authority"] = new AuthenticationProviderSettingTemplate
                        {
                            DisplayName = "Authority",
                            Description = "The authority URL for your tenant",
                            IsRequired = true
                        },
                        ["ClientId"] = new AuthenticationProviderSettingTemplate
                        {
                            DisplayName = "Client ID",
                            Description = "The application client ID from Azure AD",
                            IsRequired = true
                        },
                        ["ClientSecret"] = new AuthenticationProviderSettingTemplate
                        {
                            DisplayName = "Client Secret",
                            Description = "The application client secret from Azure AD",
                            IsRequired = true
                        },
                        ["TenantId"] = new AuthenticationProviderSettingTemplate
                        {
                            DisplayName = "Tenant ID",
                            Description = "Your Azure AD tenant ID",
                            IsRequired = false
                        }
                    }
                },
                new AuthenticationProviderTemplate
                {
                    Name = "OpenID Connect",
                    ProviderType = "OIDC",
                    Description = "Generic OpenID Connect provider",
                    Settings = new Dictionary<string, AuthenticationProviderSettingTemplate>
                    {
                        ["Authority"] = new AuthenticationProviderSettingTemplate
                        {
                            DisplayName = "Authority",
                            Description = "The OpenID Connect discovery endpoint",
                            IsRequired = true
                        },
                        ["ClientId"] = new AuthenticationProviderSettingTemplate
                        {
                            DisplayName = "Client ID",
                            Description = "The client identifier",
                            IsRequired = true
                        },
                        ["ClientSecret"] = new AuthenticationProviderSettingTemplate
                        {
                            DisplayName = "Client Secret",
                            Description = "The client secret",
                            IsRequired = true
                        }
                    }
                }
            };

            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve authentication provider templates");
            return StatusCode(500, "Internal server error");
        }
    }
}
