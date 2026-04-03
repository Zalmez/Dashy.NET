using System.Collections.Concurrent;
using System.Security.Claims;
using dashy3.Web;
using dashy3.Web.Components;
using dashy3.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddServiceDiscovery();
builder.Services.AddHttpClient("dashboards-api", static client =>
    client.BaseAddress = new("http+https://apiservice"))
    .AddServiceDiscovery();

OidcStartupConfig? oidcConfig = null;
try
{
    var apiUrl = builder.Configuration["services:apiservice:https:0"]
              ?? builder.Configuration["services:apiservice:http:0"]
              ?? "http://localhost:5000";

    using var tempClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
    tempClient.Timeout = TimeSpan.FromSeconds(5);
    var response = await tempClient.GetAsync("/api/settings/oidc/startup");
    if (response.IsSuccessStatusCode)
        oidcConfig = await response.Content.ReadFromJsonAsync<OidcStartupConfig>();
}
catch { }

var pendingSignIns = new PendingSignInStore();
builder.Services.AddSingleton(pendingSignIns);

var authBuilder = builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/login";
        options.Cookie.Name = "dashy3.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

if (oidcConfig?.IsEnabled == true && !string.IsNullOrEmpty(oidcConfig.Authority) && !string.IsNullOrEmpty(oidcConfig.ClientId) && !string.IsNullOrEmpty(oidcConfig.ClientSecret))
{
    authBuilder.AddOpenIdConnect("oidc", options =>
    {
        options.Authority = oidcConfig.Authority;
        options.ClientId = oidcConfig.ClientId;
        options.ClientSecret = oidcConfig.ClientSecret;
        options.ResponseType = "code";
        options.CallbackPath = "/signin-oidc";
        options.SaveTokens = false;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.ClaimActions.MapJsonKey("picture", "picture");
        options.ClaimActions.MapJsonKey("name", "name");

        var scopeParts = (oidcConfig.Scopes ?? "openid profile email").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        options.Scope.Clear();
        foreach (var scope in scopeParts)
            options.Scope.Add(scope);

        options.Events.OnRedirectToIdentityProvider = ctx =>
        {
            var log = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("OidcFlow");
            log.LogInformation("OIDC [1/5] Redirecting to identity provider: {Authority}", options.Authority);
            return Task.CompletedTask;
        };

        options.Events.OnMessageReceived = ctx =>
        {
            var log = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("OidcFlow");
            log.LogInformation("OIDC [2/5] Callback received from IdP. Code present: {HasCode}", !string.IsNullOrEmpty(ctx.ProtocolMessage?.Code));
            return Task.CompletedTask;
        };

        options.Events.OnTokenValidated = ctx =>
        {
            var log = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("OidcFlow");
            var allClaims = ctx.Principal?.Claims.Select(c => $"{c.Type}={c.Value}").ToList() ?? [];
            log.LogInformation("OIDC [3/5] Token validated. Claims: [{Claims}]", string.Join(", ", allClaims));
            return Task.CompletedTask;
        };

        options.Events.OnUserInformationReceived = ctx =>
        {
            var log = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("OidcFlow");
            log.LogInformation("OIDC [4/5] UserInfo endpoint responded.");
            return Task.CompletedTask;
        };

        options.Events.OnRemoteFailure = ctx =>
        {
            var log = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("OidcFlow");
            log.LogError(ctx.Failure, "OIDC remote failure: {Message}", ctx.Failure?.Message);

            var rawMsg = ctx.Failure?.Message ?? "SSO authentication failed.";
            var firstSentence = rawMsg.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)[0];
            if (firstSentence.Length > 300) firstSentence = firstSentence[..300] + "…";
            var encoded = Uri.EscapeDataString(firstSentence);

            ctx.Response.Redirect($"/login?error=oidc_failed&error_description={encoded}");
            ctx.HandleResponse();
            return Task.CompletedTask;
        };

        options.Events.OnTicketReceived = async ctx =>
        {
            var log = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("OidcFlow");

            var principal = ctx.Principal;
            if (principal is null)
            {
                log.LogError("OIDC [5/5] OnTicketReceived: principal is null");
                ctx.Fail("No principal");
                return;
            }

            var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? principal.FindFirstValue("sub") ?? "";

            var email = principal.FindFirstValue(ClaimTypes.Email)
                     ?? principal.FindFirstValue("email")
                     ?? principal.FindFirstValue("preferred_username")
                     ?? principal.FindFirstValue(ClaimTypes.Upn)
                     ?? principal.FindFirstValue("upn")
                     ?? principal.FindFirstValue("unique_name")
                     ?? "";

            var name = principal.FindFirstValue("name")
                    ?? principal.FindFirstValue(ClaimTypes.Name)
                    ?? principal.FindFirstValue(ClaimTypes.GivenName)
                    ?? email;

            var avatarUrl = principal.FindFirstValue("picture");

            log.LogInformation("OIDC [5/5] OnTicketReceived. Sub: '{Sub}', Email: '{Email}', Name: '{Name}'", sub, email, name);

            if (string.IsNullOrEmpty(sub))
            {
                log.LogError("OIDC missing sub claim. All claims: [{Claims}]",
                    string.Join(", ", principal.Claims.Select(c => $"{c.Type}={c.Value}")));
                ctx.Fail("OIDC provider did not return a subject (sub) claim.");
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                email = $"oidc_{sub}@unknown.local";
                log.LogWarning("OIDC no email claim found; using placeholder '{Email}'. Configure optional claims in your IdP to include email/upn.", email);
            }

            try
            {
                var httpFactory = ctx.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
                var http = httpFactory.CreateClient("dashboards-api");
                log.LogInformation("OIDC calling POST /api/auth/oidc-login for sub={Sub}", sub);
                var res = await http.PostAsJsonAsync("/api/auth/oidc-login", new { sub, email, name, avatarUrl });

                if (!res.IsSuccessStatusCode)
                {
                    var body = await res.Content.ReadAsStringAsync();
                    log.LogError("OIDC oidc-login API returned {StatusCode}: {Body}", (int)res.StatusCode, body);
                    ctx.Fail($"OIDC user upsert failed ({(int)res.StatusCode}): {body}");
                    return;
                }

                var userDto = await res.Content.ReadFromJsonAsync<OidcUserDto>();
                if (userDto is null)
                {
                    log.LogError("OIDC oidc-login API returned empty/null response body");
                    ctx.Fail("OIDC user upsert returned empty response");
                    return;
                }

                log.LogInformation("OIDC user upserted successfully. DB userId={UserId}, Email={Email}", userDto.Id, userDto.Email);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, userDto.Id),
                    new(ClaimTypes.Email, userDto.Email),
                    new(ClaimTypes.Name, userDto.DisplayName),
                };
                foreach (var role in userDto.Roles)
                    claims.Add(new(ClaimTypes.Role, role));
                if (!string.IsNullOrEmpty(userDto.AvatarUrl))
                    claims.Add(new("avatar_url", userDto.AvatarUrl));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ctx.Principal = new ClaimsPrincipal(identity);
                ctx.Properties!.IsPersistent = true;
                ctx.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
                ctx.Success();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "OIDC ticket processing exception: {Message}", ex.Message);
                ctx.Fail($"OIDC ticket processing exception: {ex.Message}");
            }
        };
    });
}

builder.Services.AddSingleton(new OidcFeatureFlag(oidcConfig?.IsEnabled == true));

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IDashboardService>(sp =>
{
    var factory      = sp.GetRequiredService<IHttpClientFactory>();
    var toast        = sp.GetRequiredService<IToastService>();
    var authProvider = sp.GetRequiredService<AuthenticationStateProvider>();
    var logger       = sp.GetRequiredService<ILogger<ApiDashboardService>>();
    return new ApiDashboardService(factory, toast, authProvider, logger);
});

builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IToastService, ToastService>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.MapMethods("/logout", ["GET", "POST"], async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});


app.MapGet("/auth/session", async (string token, string? returnUrl, HttpContext ctx, PendingSignInStore store) =>
{
    var ticket = store.Consume(token);
    if (ticket is null)
        return Results.Redirect("/login");

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, ticket.Id),
        new(ClaimTypes.Email, ticket.Email),
        new(ClaimTypes.Name, ticket.DisplayName),
    };
    foreach (var role in ticket.Roles)
        claims.Add(new Claim(ClaimTypes.Role, role));
    if (!string.IsNullOrEmpty(ticket.AvatarUrl))
        claims.Add(new Claim("avatar_url", ticket.AvatarUrl));

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity),
        new AuthenticationProperties { IsPersistent = true });

    var destination = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
    return Results.Redirect(destination);
});

app.MapGet("/challenge/oidc", (string? returnUrl, HttpContext ctx) =>
{
    var redirectUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
    return Results.Challenge(
        new AuthenticationProperties { RedirectUri = redirectUrl },
        ["oidc"]);
});

app.MapGet("/restarting", (HttpContext ctx) =>
{
    const string html = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>Restarting — dashy3</title>
          <style>
            *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
            body { min-height: 100vh; display: flex; align-items: center; justify-content: center;
                   background: #0e0e10; font-family: system-ui, -apple-system, sans-serif; color: #e0e0f0; }
            .card { text-align: center; max-width: 380px; padding: 2.5rem 2rem; }
            .spinner { width: 48px; height: 48px; border: 3px solid #2a2a3e; border-top-color: #7c5cbf;
                       border-radius: 50%; animation: spin 0.9s linear infinite; margin: 0 auto 1.5rem; }
            @keyframes spin { to { transform: rotate(360deg); } }
            h1 { font-size: 1.25rem; font-weight: 600; margin-bottom: 0.5rem; }
            p  { font-size: 0.875rem; color: #8b8b9e; line-height: 1.5; }
            .note { margin-top: 1.5rem; padding: 0.75rem 1rem; background: #1a1a2e;
                    border: 1px solid #2a2a3e; border-radius: 0.5rem; font-size: 0.75rem; color: #8b8b9e; }
          </style>
        </head>
        <body>
          <div class="card">
            <div class="spinner"></div>
            <h1>Application Restarting</h1>
            <p>OIDC settings are being applied. You will be redirected automatically when the application is back online.</p>
            <div class="note">
              If using Aspire, restart the <strong>webfrontend</strong> resource from the dashboard if this takes more than 10 seconds.
            </div>
          </div>
          <script>
            const returnUrl = new URLSearchParams(location.search).get('returnUrl') || '/admin';
            let attempts = 0;
            const maxAttempts = 60;
            async function poll() {
              attempts++;
              if (attempts > maxAttempts) {
                document.querySelector('p').textContent = 'The application is taking longer than expected. Please refresh manually.';
                document.querySelector('.spinner').style.display = 'none';
                return;
              }
              try {
                const res = await fetch('/health', { cache: 'no-store' });
                if (res.ok) { location.href = returnUrl; return; }
              } catch { /* still down */ }
              setTimeout(poll, 1500);
            }
            setTimeout(poll, 2000);
          </script>
        </body>
        </html>
        """;
    ctx.Response.Headers["Cache-Control"] = "no-store";
    return Results.Content(html, "text/html");
});


app.MapPost("/management/restart", async (HttpContext ctx, IHostApplicationLifetime lifetime) =>
{
    if (!ctx.User.IsInRole("Admin"))
        return Results.Forbid();

    _ = Task.Run(async () =>
    {
        await Task.Delay(800);
        lifetime.StopApplication();
    });

    return Results.Ok(new { message = "Application shutting down for restart." });
});

app.Run();

record OidcStartupConfig(string Authority, string ClientId, string ClientSecret, string? Scopes, bool IsEnabled);
record OidcUserDto(string Id, string Email, string DisplayName, string? AvatarUrl, string[] Roles, DateTime CreatedAt);
public record OidcFeatureFlag(bool IsEnabled);

public record SignInTicket(string Id, string Email, string DisplayName, string? AvatarUrl, string[] Roles);

public class PendingSignInStore
{
    private readonly ConcurrentDictionary<string, (SignInTicket Ticket, DateTime Expires)> _store = new();

    public string Store(SignInTicket ticket)
    {
        var expired = _store.Where(kvp => kvp.Value.Expires < DateTime.UtcNow).Select(kvp => kvp.Key).ToList();
        foreach (var key in expired) _store.TryRemove(key, out _);

        var token = Guid.NewGuid().ToString("N");
        _store[token] = (ticket, DateTime.UtcNow.AddSeconds(60));
        return token;
    }

    public SignInTicket? Consume(string token)
    {
        if (_store.TryRemove(token, out var entry) && entry.Expires > DateTime.UtcNow)
            return entry.Ticket;
        return null;
    }
}
