using Dashy.Net.ApiService.Services;
using Dashy.Net.ApiService.Authorization;
using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;

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

builder.Services.AddSingleton<Dashy.Net.ApiService.Services.ApiEditLockService>();

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

builder.Services.AddOpenApi();
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

if (app.Services.GetService<IAuthenticationSchemeProvider>() != null)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
