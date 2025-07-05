using Dashy.Net.Shared.Models;

namespace Dashy.Net.ApiService.Services;

public class AuthenticationProviderTemplateService
{
    private readonly Dictionary<string, AuthenticationProviderTemplate> _templates;

    public AuthenticationProviderTemplateService()
    {
        _templates = new Dictionary<string, AuthenticationProviderTemplate>
        {
            ["AzureAD"] = new AuthenticationProviderTemplate
            {
                Name = "Azure Active Directory",
                ProviderType = "AzureAD",
                Description = "Microsoft Azure Active Directory / Entra ID authentication",
                Settings = new Dictionary<string, AuthenticationProviderSettingTemplate>
                {
                    ["TenantId"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Tenant ID",
                        Description = "Your Azure AD tenant ID (Directory ID)",
                        IsRequired = true,
                        InputType = "text"
                    },
                    ["ClientId"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Client ID",
                        Description = "Application (client) ID from Azure AD app registration",
                        IsRequired = true,
                        InputType = "text"
                    },
                    ["ClientSecret"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Client Secret",
                        Description = "Client secret from Azure AD app registration",
                        IsRequired = true,
                        IsSecret = true,
                        IsEncrypted = true,
                        InputType = "password"
                    },
                    ["Authority"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Authority URL",
                        Description = "OAuth authority URL (auto-generated from tenant ID)",
                        IsRequired = false,
                        InputType = "url"
                    }
                }
            },
            ["GenericOAuth"] = new AuthenticationProviderTemplate
            {
                Name = "Generic OAuth 2.0",
                ProviderType = "GenericOAuth",
                Description = "Generic OAuth 2.0 / OpenID Connect provider",
                Settings = new Dictionary<string, AuthenticationProviderSettingTemplate>
                {
                    ["Authority"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Authority URL",
                        Description = "OAuth 2.0 authority/issuer URL",
                        IsRequired = true,
                        InputType = "url"
                    },
                    ["ClientId"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Client ID",
                        Description = "OAuth client identifier",
                        IsRequired = true,
                        InputType = "text"
                    },
                    ["ClientSecret"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Client Secret",
                        Description = "OAuth client secret",
                        IsRequired = true,
                        IsSecret = true,
                        IsEncrypted = true,
                        InputType = "password"
                    },
                    ["Scopes"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Scopes",
                        Description = "OAuth scopes (space-separated)",
                        DefaultValue = "openid profile email",
                        IsRequired = false,
                        InputType = "text"
                    }
                }
            },
            ["Authentik"] = new AuthenticationProviderTemplate
            {
                Name = "Authentik",
                ProviderType = "Authentik",
                Description = "Authentik identity provider",
                Settings = new Dictionary<string, AuthenticationProviderSettingTemplate>
                {
                    ["Authority"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Authentik URL",
                        Description = "Base URL of your Authentik instance",
                        IsRequired = true,
                        InputType = "url"
                    },
                    ["ClientId"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Client ID",
                        Description = "Application client ID from Authentik",
                        IsRequired = true,
                        InputType = "text"
                    },
                    ["ClientSecret"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Client Secret",
                        Description = "Application client secret from Authentik",
                        IsRequired = true,
                        IsSecret = true,
                        IsEncrypted = true,
                        InputType = "password"
                    },
                    ["Scopes"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Scopes",
                        Description = "OAuth scopes (space-separated)",
                        DefaultValue = "openid profile email",
                        IsRequired = false,
                        InputType = "text"
                    }
                }
            },
            ["Keycloak"] = new AuthenticationProviderTemplate
            {
                Name = "Keycloak",
                ProviderType = "Keycloak",
                Description = "Red Hat Keycloak identity provider",
                Settings = new Dictionary<string, AuthenticationProviderSettingTemplate>
                {
                    ["Authority"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Keycloak URL",
                        Description = "Keycloak realm URL (e.g., https://keycloak.example.com/realms/myrealm)",
                        IsRequired = true,
                        InputType = "url"
                    },
                    ["ClientId"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Client ID",
                        Description = "Client ID from Keycloak",
                        IsRequired = true,
                        InputType = "text"
                    },
                    ["ClientSecret"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Client Secret",
                        Description = "Client secret from Keycloak",
                        IsRequired = true,
                        IsSecret = true,
                        IsEncrypted = true,
                        InputType = "password"
                    },
                    ["Scopes"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Scopes",
                        Description = "OAuth scopes (space-separated)",
                        DefaultValue = "openid profile email",
                        IsRequired = false,
                        InputType = "text"
                    }
                }
            },
            ["Auth0"] = new AuthenticationProviderTemplate
            {
                Name = "Auth0",
                ProviderType = "Auth0",
                Description = "Auth0 identity provider",
                Settings = new Dictionary<string, AuthenticationProviderSettingTemplate>
                {
                    ["Domain"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Domain",
                        Description = "Auth0 domain (e.g., your-domain.auth0.com)",
                        IsRequired = true,
                        InputType = "text"
                    },
                    ["ClientId"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Client ID",
                        Description = "Application client ID from Auth0",
                        IsRequired = true,
                        InputType = "text"
                    },
                    ["ClientSecret"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Client Secret",
                        Description = "Application client secret from Auth0",
                        IsRequired = true,
                        IsSecret = true,
                        IsEncrypted = true,
                        InputType = "password"
                    },
                    ["Authority"] = new AuthenticationProviderSettingTemplate
                    {
                        DisplayName = "Authority URL",
                        Description = "Auth0 authority URL (auto-generated from domain)",
                        IsRequired = false,
                        InputType = "url"
                    }
                }
            }
        };
    }

    public IEnumerable<AuthenticationProviderTemplate> GetAllTemplates()
    {
        return _templates.Values;
    }

    public AuthenticationProviderTemplate? GetTemplate(string providerType)
    {
        return _templates.TryGetValue(providerType, out var template) ? template : null;
    }

    public IEnumerable<string> GetSupportedProviderTypes()
    {
        return _templates.Keys;
    }
}