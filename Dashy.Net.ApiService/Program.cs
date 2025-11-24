using Dashy.Net.ApiService.Services;
using Dashy.Net.ApiService.Authorization;
using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Serialization;
using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Reflection;
using System.Diagnostics;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
    });

builder.AddServiceDefaults();
var cnnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__dashy");
builder.Services.AddNpgsql<AppDbContext>(cnnectionString);
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("Short", p => p.Expire(TimeSpan.FromSeconds(30)).Tag("version"));
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("standard", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

builder.Services.AddHttpClient("WeatherApi", client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/");
});

var authAuthority = Environment.GetEnvironmentVariable("auth_authority");
var authClientId = Environment.GetEnvironmentVariable("auth_clientid");
var authClientSecret = Environment.GetEnvironmentVariable("auth_clientsecret");
var isAuthConfigured = !string.IsNullOrWhiteSpace(authAuthority) && 
                      !string.IsNullOrWhiteSpace(authClientId) && 
                      !string.IsNullOrWhiteSpace(authClientSecret);

if (isAuthConfigured)
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

        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = authAuthority;
        options.Audience = authClientId;
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        options.TokenValidationParameters.RoleClaimType = "roles";
    });
}

builder.Services.AddSingleton<ApiEditLockService>();

builder.Services.AddSingleton<IAuthorizationHandler>(provider => 
    new ConditionalAuthorizationHandler());

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddRequirements(new ConditionalAuthorizationRequirement(isAuthConfigured))
        .Build();
    
    options.AddPolicy("Admin", policy =>
        policy.AddRequirements(new ConditionalAuthorizationRequirement(isAuthConfigured)));
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (isAuthConfigured)
{
    logger.LogInformation("Authentication is ENABLED. API endpoints require valid JWT Bearer tokens.");
}
else
{
    logger.LogWarning("Authentication is DISABLED. All API endpoints are accessible without authentication.");
    if (string.IsNullOrWhiteSpace(authAuthority))
        logger.LogWarning("To enable authentication, set the 'auth_authority' environment variable.");
    if (string.IsNullOrWhiteSpace(authClientId))
        logger.LogWarning("To enable authentication, set the 'auth_clientid' environment variable.");
    if (string.IsNullOrWhiteSpace(authClientSecret))
        logger.LogWarning("To enable authentication, set the 'auth_clientsecret' environment variable.");
}
app.UseExceptionHandler();

var webRoot = app.Environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
if (!Directory.Exists(webRoot))
    Directory.CreateDirectory(webRoot);

app.UseStaticFiles();
app.UseRateLimiter();
app.UseOutputCache();

if (app.Services.GetService<IAuthenticationSchemeProvider>() != null)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapOpenApi();
app.MapScalarApiReference();

var v1 = app.MapGroup("/api/v1").RequireRateLimiting("standard");

v1.MapGet("/version", (HttpContext httpContext) =>
{
    var assembly = Assembly.GetExecutingAssembly();
    try
    {
        string GetVersionPart()
        {
            var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!string.IsNullOrEmpty(informational)) return informational.Split('+')[0];
            var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            if (!string.IsNullOrEmpty(fileVersion)) return fileVersion;
            var av = assembly.GetName().Version;
            return av is null ? "Unknown" : $"{av.Major}.{av.Minor}.{av.Build}";
        }
        string GetFullVersion() => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown";
        bool IsPreRelease(string v, string full) => v.Contains('-') || full.Contains('-') || v.Contains("dev") || full.Contains("dev");
        string? CommitHash()
        {
            var inf = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (inf?.Contains('+') == true)
            {
                var parts = inf.Split('+');
                if (parts.Length > 1) return parts[1].Split('.')[0];
            }
            return null;
        }
        DateTime? BuildDate()
        {
            var loc = assembly.Location;
            return File.Exists(loc) ? File.GetCreationTimeUtc(loc) : null;
        }

        var version = GetVersionPart();
        var fullVersion = GetFullVersion();
        var info = new ServiceVersionInfo
        {
            ServiceName = "ApiService",
            Version = version,
            FullVersion = fullVersion,
            IsPreRelease = IsPreRelease(version, fullVersion),
            CommitHash = CommitHash(),
            BuildDate = BuildDate()
        };
        Activity.Current?.AddTag("dashy.version", version);
        return Results.Ok(info);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to retrieve version information");
        return Results.Problem("Failed to retrieve version information");
    }
})
.CacheOutput("Short")
.WithName("GetVersionV1")
.WithSummary("Gets API version information")
.WithDescription("Returns version metadata for the API service.");

app.MapControllers();

using(var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
