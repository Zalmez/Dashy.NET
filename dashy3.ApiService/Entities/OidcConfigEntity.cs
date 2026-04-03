namespace dashy3.ApiService.Entities;

public class OidcConfigEntity
{
    public int Id { get; set; } = 1; // Singleton row
    public string Authority { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string Scopes { get; set; } = "openid profile email";
    public string CallbackPath { get; set; } = "/signin-oidc";
    public bool IsEnabled { get; set; } = false;
    public bool AllowAutoRegistration { get; set; } = true; // When false, SSO users must be pre-invited
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
