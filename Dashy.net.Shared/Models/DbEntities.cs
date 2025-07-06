using System.ComponentModel.DataAnnotations;

namespace Dashy.Net.Shared.Models;

public class DashboardItem
{
    public int Id { get; set; }
    [Required]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Url {get; set;}
    public string? Icon { get; set; }
    public string Widget { get; set; } = "static-link";
    public string? OptionsJson { get; set; }

    public int SectionId { get; set; }
    public DashboardSection Section { get; set; } = null!;
    public int Position { get; set; } = 0;

}

public class DashboardSection
{
    public int Id { get; set; }
    [Required]
    public required string Name { get; set; }
    public string? Icon { get; set; }
    public List<DashboardItem> Items { get; set; } = [];
    public int Position { get; set; } = 0;
    public int DashboardId { get; set; }
    public Dashboard Dashboard { get; set; } = null!;
}

public class Dashboard
{
    public int Id { get; set; }
    [Required]
    public required string Title { get; set; } = "My Dashboard";
    public string? Subtitle { get; set; }
    public List<DashboardSection> Sections { get; set; } = [];
    public List<HeaderButton> HeaderButtons { get; set; } = [];
}

public class HeaderButton
{
    public int Id { get; set; }
    [Required]
    public required string Text { get; set; }
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public int Position { get; set; }

    public int DashboardId { get; set; }
    public Dashboard Dashboard { get; set; } = null!;
}

public class AuthenticationSettings
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; } = false;
    public string? Authority { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? TenantId { get; set; }
    public string? Scopes { get; set; } // Added Scopes field
}

public class AuthenticationProvider
{
    public int Id { get; set; }
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }
    [Required]
    [StringLength(50)]
    public required string ProviderType { get; set; }
    public bool IsEnabled { get; set; } = false;
    public bool IsDefault { get; set; } = false;
    public int Priority { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public List<AuthenticationProviderSettings> Settings { get; set; } = [];
}

public class AuthenticationProviderSettings
{
    public int Id { get; set; }
    public int AuthenticationProviderId { get; set; }
    [Required]
    [StringLength(100)]
    public required string Key { get; set; }
    [StringLength(2000)]
    public string? Value { get; set; }
    public bool IsEncrypted { get; set; } = false;
    public bool IsRequired { get; set; } = false;
    
    public AuthenticationProvider AuthenticationProvider { get; set; } = null!;
}

public enum AuthenticationProviderType
{
    AzureAD,
    GenericOAuth,
    OpenIdConnect,
    Authentik,
    Keycloak,
    Auth0
}