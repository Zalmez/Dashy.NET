using dashy3.Web.Models;

namespace dashy3.Web.Services;

public interface IAuthService
{
    Task<(bool success, string? error, string? sessionToken)> LoginAsync(string email, string password);
    Task<(bool success, string? error, string? sessionToken)> RegisterAsync(string email, string password, string displayName, string role = "User");
    Task LogoutAsync();
    Task<UserInfo?> GetCurrentUserAsync();
    Task<(bool success, string? error)> UpdateProfileAsync(string displayName, string? avatarUrl);
    Task<(bool success, string? error)> ChangePasswordAsync(string currentPassword, string newPassword);
    Task<bool> NeedsSetupAsync();
    Task<UserInfo?> OidcLoginAsync(string sub, string email, string name, string? avatarUrl);
    /// <summary>
    /// Returns true only when the user has a valid auth cookie but their record no longer exists in the DB (404).
    /// Returns false if not authenticated, if the user is found, or if the API is unavailable (avoids false positives).
    /// </summary>
    Task<bool> IsGhostSessionAsync();
}
