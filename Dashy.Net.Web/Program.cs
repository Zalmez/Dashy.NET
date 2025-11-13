using BlazorSortable;
using Dashy.Net.Web.Clients;
using Dashy.Net.Web.Components;
using Dashy.Net.Web.Helpers;
using Dashy.Net.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenTelemetry();
builder.Services.AddProblemDetails();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

#region Dashy.Net required services
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenHandler>();

builder.Services.AddHttpClient<DashboardClient>(opts =>
{
    opts.BaseAddress = new("https+http://apiservice");
}).AddHttpMessageHandler<TokenHandler>();

builder.Services.AddHttpClient<WeatherClient>(opts =>
{
    opts.BaseAddress = new("https+http://apiservice");
}).AddHttpMessageHandler<TokenHandler>();

builder.Services.AddHttpClient<EditLocksClient>(opts =>
{
    opts.BaseAddress = new("https+http://apiservice");
}).AddHttpMessageHandler<TokenHandler>();

builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<ViewOptionsService>();
builder.Services.AddScoped<ClientStorageService>();
builder.Services.AddSingleton<EditLockService>();
builder.Services.AddSingleton<DashboardSyncService>();
builder.Services.AddSortable();
builder.Services.AddScoped<DashboardStateService>();
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddSingleton<WidgetRegistryService>();
builder.Services.AddSingleton<IVersionService, VersionService>();
builder.Services.AddHttpClient("ApiService", opts =>
{
    opts.BaseAddress = new("https+http://apiservice");
}).AddHttpMessageHandler<TokenHandler>();

builder.Services.AddTransient<EventSubscriptionManager>();
builder.Services.AddHttpClient<AppSettingsClient>(opts =>
{
    opts.BaseAddress = new("https+http://apiservice");
}).AddHttpMessageHandler<TokenHandler>();
#endregion


var authAuthority = Environment.GetEnvironmentVariable("auth_authority");
var authClientId = Environment.GetEnvironmentVariable("auth_clientid");
var authClientSecret = Environment.GetEnvironmentVariable("auth_clientsecret");

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
var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
var startupConfig = app.Services.GetRequiredService<IConfiguration>();
var startupStoragePath = startupConfig["DASHYDOTNET_STORAGE_PATH"];

if (!string.IsNullOrWhiteSpace(startupStoragePath))
{
    startupLogger.LogInformation("=== Azure File Storage Configuration ===");
    startupLogger.LogInformation("DASHYDOTNET_STORAGE_PATH: {StoragePath}", startupStoragePath);
    startupLogger.LogInformation("Storage path exists: {Exists}", Directory.Exists(startupStoragePath));
    
    if (Directory.Exists(startupStoragePath))
    {
        try
        {
            var files = Directory.GetFiles(startupStoragePath);
            startupLogger.LogInformation("Files in storage: {FileCount}", files.Length);
            foreach (var file in files.Take(10))
            {
                startupLogger.LogInformation("  - {FileName}", Path.GetFileName(file));
            }
        }
        catch (Exception ex)
        {
            startupLogger.LogError(ex, "Error listing files in storage path");
        }
    }
    startupLogger.LogInformation("========================================");
}
else
{
    startupLogger.LogInformation("Using default wwwroot storage (no DASHYDOTNET_STORAGE_PATH configured)");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Avoid redirecting health endpoints so Aspire HTTP health checks work
app.UseWhen(ctx => !(ctx.Request.Path.StartsWithSegments("/health") || ctx.Request.Path.StartsWithSegments("/alive")), branch =>
{
    branch.UseHttpsRedirection();
});

if (!string.IsNullOrWhiteSpace(authAuthority) && !string.IsNullOrWhiteSpace(authClientId) && !string.IsNullOrWhiteSpace(authClientSecret))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseAntiforgery();
app.UseOutputCache();

// Configure static file serving for uploaded files from Azure File Storage
var customStoragePath = Environment.GetEnvironmentVariable("DASHYDOTNET_STORAGE_PATH");
if (!string.IsNullOrWhiteSpace(customStoragePath) && Directory.Exists(customStoragePath))
{
    logger.LogInformation("Configuring static file serving for custom storage path: {StoragePath}", customStoragePath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(customStoragePath),
        RequestPath = "/uploads"
    });
}

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

# region Health Checks and Diagnostics
app.MapGet("/debug/storage", (IConfiguration config, ILogger<Program> logger) =>
{
    var customStoragePath = config["DASHYDOTNET_STORAGE_PATH"];
    var diagnostics = new
    {
        CustomStoragePath = customStoragePath,
        CustomStorageExists = !string.IsNullOrWhiteSpace(customStoragePath) && Directory.Exists(customStoragePath),
        CustomStorageFiles = !string.IsNullOrWhiteSpace(customStoragePath) && Directory.Exists(customStoragePath) 
            ? Directory.GetFiles(customStoragePath).Select(f => Path.GetFileName(f)).ToArray()
            : Array.Empty<string>(),
        WwwRootPath = app.Environment.WebRootPath,
        ContentRootPath = app.Environment.ContentRootPath,
        Environment = app.Environment.EnvironmentName
    };
    
    logger.LogInformation("Storage diagnostics requested: {@Diagnostics}", diagnostics);
    return Results.Json(diagnostics);
});

app.MapGet("/health/storage", (IConfiguration config) =>
{
    var customStoragePath = config["DASHYDOTNET_STORAGE_PATH"];
    
    if (string.IsNullOrWhiteSpace(customStoragePath))
    {
        return Results.Ok(new { Status = "Healthy", Message = "Using default wwwroot storage" });
    }
    
    if (!Directory.Exists(customStoragePath))
    {
        return Results.Problem($"Custom storage path does not exist: {customStoragePath}");
    }
    
    try
    {
        var testFile = Path.Combine(customStoragePath, $"health_check_{Guid.NewGuid()}.tmp");
        File.WriteAllText(testFile, "health check");
        File.Delete(testFile);
        
        return Results.Ok(new { Status = "Healthy", Message = $"Azure File Storage is accessible at {customStoragePath}" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Cannot write to storage path {customStoragePath}: {ex.Message}");
    }
});

app.MapGet("/debug/test-upload", async (IConfiguration config, FileStorageService fileStorage) =>
{
    var customStoragePath = config["DASHYDOTNET_STORAGE_PATH"];
    
    if (string.IsNullOrWhiteSpace(customStoragePath) || !Directory.Exists(customStoragePath))
    {
        return Results.Problem("Azure File Storage not configured or not accessible");
    }
    
    try
    {
        var testImageBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");
        var testFileName = $"test_{Guid.NewGuid()}.png";
        var testFilePath = Path.Combine(customStoragePath, testFileName);
        
        await File.WriteAllBytesAsync(testFilePath, testImageBytes);
        
        return Results.Json(new 
        { 
            Status = "Test image created",
            FileName = testFileName,
            PublicUrl = $"/uploads/{testFileName}",
            FilePath = testFilePath,
            FileExists = File.Exists(testFilePath),
            FileSize = new FileInfo(testFilePath).Length
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error creating test image: {ex.Message}");
    }
});
#endregion

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
