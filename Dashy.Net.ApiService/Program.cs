using Dashy.Net.ApiService.Services;
using Dashy.Net.Shared.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.Services.AddControllers();
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("dashy");
// Add services to the container.
builder.Services.AddProblemDetails();

// Register the authentication settings provider
builder.Services.AddScoped<AuthenticationSettingsProvider>();
builder.Services.AddSingleton<AuthenticationProviderTemplateService>();

// Register the one-time token service for secure authentication configuration access
builder.Services.AddSingleton<OneTimeTokenService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpClient("WeatherApi", client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/");
});

var app = builder.Build();

// Initialize the one-time token service (generates first token and logs it)
var oneTimeTokenService = app.Services.GetRequiredService<OneTimeTokenService>();
// Token is generated in the constructor and logged automatically

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Conditionally enable authentication/authorization
using (var scope = app.Services.CreateScope())
{
    var settingsProvider = scope.ServiceProvider.GetRequiredService<AuthenticationSettingsProvider>();
    var settings = await settingsProvider.GetSettingsAsync();
    if (settings is not null && settings.IsEnabled)
    {
        app.Services.GetRequiredService<IServiceCollection>()
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.Authority = settings.Authority;
                options.ClientId = settings.ClientId;
                options.ClientSecret = settings.ClientSecret;
                options.ResponseType = "code";
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
            });
        app.UseAuthentication();
        app.UseAuthorization();
    }
}

app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
