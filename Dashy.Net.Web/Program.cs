using BlazorSortable;
using Dashy.Net.Shared.Security;
using Dashy.Net.Web;
using Dashy.Net.Web.Clients;
using Dashy.Net.Web.Components;
using Dashy.Net.Web.Helpers;
using Dashy.Net.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

#region Dashy.Net required services
builder.Services.AddHttpClient<DashboardClient>(opts =>
{
    opts.BaseAddress = new("https+http://apiservice");
});
builder.Services.AddHttpClient<WeatherClient>(opts =>
{
    opts.BaseAddress = new("https+http://apiservice");
});

builder.Services.AddSingleton<ThemeService>();
builder.Services.AddSingleton<ViewOptionsService>();
builder.Services.AddSortableServices();
builder.Services.AddSingleton<DashboardStateService>();
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddSingleton<WidgetRegistryService>();

builder.Services.AddHttpClient("ApiService", opts =>
{
    opts.BaseAddress = new("https+http://apiservice");
});
builder.Services.AddScoped<AuthenticationSettingsProvider>();
builder.Services.AddScoped<EventSubscriptionManager>();
#endregion

Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true; //TODO: REMOVE THIS
var authConfigured = false;
try
{
    var tempProvider = builder.Services.BuildServiceProvider();
    var settingsProvider = tempProvider.GetRequiredService<AuthenticationSettingsProvider>();
    var settings = await settingsProvider.GetSettingsAsync();
    
    if (settings is not null && settings.IsEnabled && 
        !string.IsNullOrWhiteSpace(settings.Authority) && 
        !string.IsNullOrWhiteSpace(settings.ClientId))
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/authentication/login";
            options.LogoutPath = "/authentication/logout";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Authority = settings.Authority;
            options.ClientId = settings.ClientId;
            options.ClientSecret = settings.ClientSecret;
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.ResponseType = OpenIdConnectResponseType.Code;

            // Add scopes
            if (!string.IsNullOrEmpty(settings.Scopes))
            {
                foreach (var _scope in settings.Scopes.Split(" "))
                {
                    options.Scope.Add(_scope.ToLower());
                }
            }

            options.MapInboundClaims = false;
            options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        });
        
        authConfigured = true;
    }
}
catch (Exception ex)
{
    // Log authentication configuration failure but don't crash the app
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Failed to configure authentication. The application will start without authentication.");
}


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add authentication middleware if configured
if (authConfigured)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseAntiforgery();
app.UseOutputCache();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add authentication endpoints if configured
if (authConfigured)
{
    app.MapGet("/authentication/login", async (HttpContext context) =>
    {
        await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
    });

    app.MapGet("/authentication/logout", async (HttpContext context) =>
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    });
}

app.MapDefaultEndpoints();

app.Run();
