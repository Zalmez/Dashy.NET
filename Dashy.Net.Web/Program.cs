using BlazorSortable;
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
builder.Services.AddTransient<EventSubscriptionManager>();
#endregion

var authAuthority = Environment.GetEnvironmentVariable("auth_authority");
var authClientId = Environment.GetEnvironmentVariable("auth_clientid");
var authClientSecret = Environment.GetEnvironmentVariable("auth_clientsecret");

if (!string.IsNullOrWhiteSpace(authAuthority) && !string.IsNullOrWhiteSpace(authClientId) && !string.IsNullOrWhiteSpace(authClientSecret))
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
        options.Authority = authAuthority;
        options.ClientId = authClientId;
        options.ClientSecret = authClientSecret;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.ResponseType = OpenIdConnectResponseType.Code;

        // Add default scopes
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
    });
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add authentication middleware if configured
if (!string.IsNullOrWhiteSpace(authAuthority) && !string.IsNullOrWhiteSpace(authClientId) && !string.IsNullOrWhiteSpace(authClientSecret))
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
if (!string.IsNullOrWhiteSpace(authAuthority) && !string.IsNullOrWhiteSpace(authClientId) && !string.IsNullOrWhiteSpace(authClientSecret))
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
