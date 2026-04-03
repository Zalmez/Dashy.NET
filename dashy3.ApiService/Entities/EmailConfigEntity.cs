namespace dashy3.ApiService.Entities;

/// <summary>
/// Configuration for sending invite/notification emails.
/// Supports SMTP or Microsoft Graph API.
/// </summary>
public class EmailConfigEntity
{
    public int Id { get; set; } = 1; // Singleton row
    
    /// <summary>
    /// Email provider: "None", "Smtp", or "MicrosoftGraph"
    /// </summary>
    public string Provider { get; set; } = "None";
    
    public bool IsEnabled { get; set; } = false;
    
    // ── SMTP Settings ──────────────────────────────────────────────────────────
    public string SmtpHost { get; set; } = "";
    public int SmtpPort { get; set; } = 587;
    public bool SmtpUseSsl { get; set; } = true;
    public string SmtpUsername { get; set; } = "";
    public string SmtpPassword { get; set; } = "";
    public string SmtpFromEmail { get; set; } = "";
    public string SmtpFromName { get; set; } = "Dashy";
    
    // ── Microsoft Graph Settings ───────────────────────────────────────────────
    public string GraphTenantId { get; set; } = "";
    public string GraphClientId { get; set; } = "";
    public string GraphClientSecret { get; set; } = "";
    public string GraphFromEmail { get; set; } = ""; // Must be a valid mailbox in the tenant
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
