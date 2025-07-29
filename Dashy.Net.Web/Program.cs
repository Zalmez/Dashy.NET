using BlazorSortable;
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
builder.Services.AddHttpClient<EditLocksClient>(opts =>
{
    opts.BaseAddress = new("https+http://apiservice");
});
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddScoped<ViewOptionsService>();
builder.Services.AddSingleton<EditLockService>();
builder.Services.AddSingleton<DashboardSyncService>();
builder.Services.AddSortableServices();
builder.Services.AddSingleton<DashboardStateService>();
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddSingleton<WidgetRegistryService>();
builder.Services.AddSingleton<IVersionService, VersionService>();
builder.Services.AddHttpClient("ApiService", opts =>
{
    opts.BaseAddress = new("https+http://apiservice");
});
builder.Services.AddTransient<EventSubscriptionManager>();
#endregion


var authAuthority = Environment.GetEnvironmentVariable("auth_authority");
var authClientId = Environment.GetEnvironmentVariable("auth_clientid");
var authClientSecret = Environment.GetEnvironmentVariable("auth_clientsecret");

//build the logger so we can log warnings if auth settings are not provided
var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});
builder.Services.AddSingleton<ILoggerFactory>(loggerFactory);

var logger = loggerFactory.CreateLogger("Webfrontend.Program");

if (!string.IsNullOrWhiteSpace(authAuthority) && !string.IsNullOrWhiteSpace(authClientId) && !string.IsNullOrWhiteSpace(authClientSecret))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.UseIfAvailable;
        options.Authority = authAuthority;
        options.ClientId = authClientId;
        options.ClientSecret = authClientSecret;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.ResponseType = OpenIdConnectResponseType.Code;

        options.Scope.Add(OpenIdConnectScope.OpenIdProfile);
        options.Scope.Add(OpenIdConnectScope.OfflineAccess);
        options.Scope.Add(OpenIdConnectScope.OpenId);
        options.Scope.Add(OpenIdConnectScope.Email);

        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "roles";
        options.CallbackPath = new PathString("/signin-oidc");
        options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
        options.RemoteSignOutPath = new PathString("/signout-oidc");
        options.ReturnUrlParameter = "ReturnUrl";
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.ReturnUrlParameter = "ReturnUrl";
    });

}
else
{
    if (string.IsNullOrWhiteSpace(authAuthority))
        logger.LogWarning("Authentication authority is not set. Please set the environment variable 'auth_authority' to enable authentication.");
    if (string.IsNullOrWhiteSpace(authClientId))
        logger.LogWarning("Authentication client ID is not set. Please set the environment variable 'auth_clientid' to enable authentication.");
    if (string.IsNullOrWhiteSpace(authClientSecret))
        logger.LogWarning("Authentication client secret is not set. Please set the environment variable 'auth_clientsecret' to enable authentication.");
}
builder.AddServiceDefaults();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

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

if (!string.IsNullOrWhiteSpace(authAuthority) && !string.IsNullOrWhiteSpace(authClientId) && !string.IsNullOrWhiteSpace(authClientSecret))
{
    app.MapGet("/authentication/login", async (HttpContext context, string? returnUrl) =>
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/"
        };
        await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, props);
    });

    app.MapGet("/authentication/logout", async (HttpContext context) =>
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    });
}

app.MapDefaultEndpoints();

app.Run();
