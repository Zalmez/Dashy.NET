namespace dashy3.ApiService.Dtos;

public record OidcConfigDto(
    string Authority,
    string ClientId,
    string ClientSecret, // Returned as empty string from GET for security
    string Scopes,
    string CallbackPath,
    bool IsEnabled,
    bool AllowAutoRegistration,
    DateTime UpdatedAt);

public record UpdateOidcConfigRequest(
    string Authority,
    string ClientId,
    string? ClientSecret, // Null means "don't change the stored secret"
    string Scopes,
    string CallbackPath,
    bool IsEnabled,
    bool AllowAutoRegistration = true);

// Returned only by the internal /settings/oidc/startup endpoint.
// Contains the real client secret for Web startup configuration.
public record OidcStartupDto(
    string Authority,
    string ClientId,
    string ClientSecret,
    string Scopes,
    string CallbackPath,
    bool IsEnabled,
    bool AllowAutoRegistration);

// ── Invite DTOs ────────────────────────────────────────────────────────────────

public record InviteDto(
    string Id,
    string Token, // Include token so admin can copy invite link
    string Email,
    string Role,
    string? InvitedByUserId,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsUsed,
    DateTime? UsedAt);

public record CreateInviteRequest(
    string Email,
    string Role = "User");

public record AcceptInviteRequest(
    string DisplayName,
    string Password);

public record InviteInfoDto(
    string Email,
    string Role,
    DateTime ExpiresAt,
    bool IsValid); // false if expired or used

// ── Email Config DTOs ──────────────────────────────────────────────────────────

public record EmailConfigDto(
    string Provider, // None, Smtp, MicrosoftGraph
    bool IsEnabled,
    string SmtpHost,
    int SmtpPort,
    bool SmtpUseSsl,
    string SmtpUsername,
    string SmtpFromEmail,
    string SmtpFromName,
    string GraphTenantId,
    string GraphClientId,
    string GraphFromEmail,
    DateTime UpdatedAt);

public record UpdateEmailConfigRequest(
    string Provider,
    bool IsEnabled,
    // SMTP
    string? SmtpHost,
    int? SmtpPort,
    bool? SmtpUseSsl,
    string? SmtpUsername,
    string? SmtpPassword, // null = don't change
    string? SmtpFromEmail,
    string? SmtpFromName,
    // Microsoft Graph
    string? GraphTenantId,
    string? GraphClientId,
    string? GraphClientSecret, // null = don't change
    string? GraphFromEmail);

public record TestEmailRequest(string ToEmail);
