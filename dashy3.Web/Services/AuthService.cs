using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using dashy3.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace dashy3.Web.Services;

public class AuthService(
    IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor,
    PendingSignInStore pendingSignIns,
    AuthenticationStateProvider authStateProvider,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient("dashboards-api");
    private readonly IHttpContextAccessor _ctx = httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<bool> NeedsSetupAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<CheckSetupResponse>("/api/auth/check-setup", JsonOpts);
            return res?.NeedsSetup ?? false;
        }
        catch { return false; }
    }

    public async Task<(bool success, string? error, string? sessionToken)> LoginAsync(string email, string password)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("/api/auth/login", new { email, password });
            if (!res.IsSuccessStatusCode)
                return (false, "Invalid email or password", null);

            var user = await res.Content.ReadFromJsonAsync<UserInfoDto>(JsonOpts);
            if (user is null) return (false, "Unexpected error", null);

            var token = CreatePendingSession(user);
            return (true, null, token);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool success, string? error, string? sessionToken)> RegisterAsync(string email, string password, string displayName, string role = "User")
    {
        try
        {
            var res = await _http.PostAsJsonAsync("/api/auth/register", new { email, password, displayName, role });
            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadFromJsonAsync<ErrorResponse>(JsonOpts);
                return (false, err?.Errors?.FirstOrDefault() ?? "Registration failed", null);
            }

            var user = await res.Content.ReadFromJsonAsync<UserInfoDto>(JsonOpts);
            if (user is null) return (false, "Unexpected error", null);

            var token = CreatePendingSession(user);
            return (true, null, token);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task LogoutAsync()
    {
        var ctx = _ctx.HttpContext;
        if (ctx is not null)
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        var ctx = _ctx.HttpContext;
        string? userId = ctx?.User?.Identity?.IsAuthenticated == true
            ? ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;

                    if (string.IsNullOrEmpty(userId))
        {
            try
            {
                var authState = await authStateProvider.GetAuthenticationStateAsync();
                if (authState.User.Identity?.IsAuthenticated == true)
                    userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            catch { /* non-critical */ }
        }

        if (string.IsNullOrEmpty(userId)) return null;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
            request.Headers.Add("X-User-Id", userId);
            var res = await _http.SendAsync(request);
            if (!res.IsSuccessStatusCode) return null;

            var dto = await res.Content.ReadFromJsonAsync<UserInfoDto>(JsonOpts);
            return dto is null ? null : MapToUserInfo(dto);
        }
        catch { return null; }
    }

    public async Task<(bool success, string? error)> UpdateProfileAsync(string displayName, string? avatarUrl)
    {
        var ctx = _ctx.HttpContext;
        var userId = ctx?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return (false, "Not authenticated");

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "/api/auth/profile");
            request.Headers.Add("X-User-Id", userId);
            request.Content = JsonContent.Create(new { displayName, avatarUrl });
            var res = await _http.SendAsync(request);
            if (!res.IsSuccessStatusCode) return (false, "Update failed");

            var user = await res.Content.ReadFromJsonAsync<UserInfoDto>(JsonOpts);
            _ = user;
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool success, string? error)> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var ctx = _ctx.HttpContext;
        var userId = ctx?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return (false, "Not authenticated");

        try
        {
            var res = await _http.PostAsJsonAsync("/api/auth/change-password",
                new { userId, currentPassword, newPassword });
            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadFromJsonAsync<ErrorResponse>(JsonOpts);
                return (false, err?.Errors?.FirstOrDefault() ?? "Password change failed");
            }
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<bool> IsGhostSessionAsync()
    {
        string? userId = null;

        var ctx = _ctx.HttpContext;
        if (ctx?.User?.Identity?.IsAuthenticated == true)
            userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            try
            {
                var authState = await authStateProvider.GetAuthenticationStateAsync();
                if (authState.User.Identity?.IsAuthenticated == true)
                    userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            catch { }
        }

                if (string.IsNullOrEmpty(userId)) return false;

                try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
            request.Headers.Add("X-User-Id", userId);
            var res = await _http.SendAsync(request);
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogWarning("Ghost session detected: userId '{UserId}' not found in database. Cookie will be cleared.", userId);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserInfo?> OidcLoginAsync(string sub, string email, string name, string? avatarUrl)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("/api/auth/oidc-login", new { sub, email, name, avatarUrl });
            if (!res.IsSuccessStatusCode) return null;

            var user = await res.Content.ReadFromJsonAsync<UserInfoDto>(JsonOpts);
            if (user is null) return null;

                        var ctx = _ctx.HttpContext;
            if (ctx is not null && !ctx.Response.HasStarted)
                await SignInUserDirect(ctx, user);

            return MapToUserInfo(user);
        }
        catch { return null; }
    }

    /// <summary>
    /// Stores user info in the pending sign-in store and returns a one-time token.
    /// The component should then navigate to /auth/session?token={token} with forceLoad:true.
    /// This is needed because HttpContext.SignInAsync cannot be called from the Blazor Server
    /// circuit (response has already started).
    /// </summary>
    private string CreatePendingSession(UserInfoDto user)
    {
        var ticket = new SignInTicket(user.Id, user.Email, user.DisplayName, user.AvatarUrl, user.Roles);
        return pendingSignIns.Store(ticket);
    }

    /// <summary>Signs in directly via HttpContext — only safe in real HTTP middleware (e.g. OIDC callback).</summary>
    private static async Task SignInUserDirect(HttpContext ctx, UserInfoDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
        };
        foreach (var role in user.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        if (!string.IsNullOrEmpty(user.AvatarUrl))
            claims.Add(new Claim("avatar_url", user.AvatarUrl));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }

    private static UserInfo MapToUserInfo(UserInfoDto dto) => new()
    {
        Id = dto.Id,
        Email = dto.Email,
        DisplayName = dto.DisplayName,
        AvatarUrl = dto.AvatarUrl,
        Roles = dto.Roles,
        CreatedAt = dto.CreatedAt,
        LastLoginAt = dto.LastLoginAt
    };

    private record UserInfoDto(string Id, string Email, string DisplayName, string? AvatarUrl, string[] Roles, DateTime CreatedAt, DateTime? LastLoginAt);
    private record CheckSetupResponse(bool NeedsSetup);
    private record ErrorResponse(string[]? Errors);
}
