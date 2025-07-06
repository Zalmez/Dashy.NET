using Dashy.Net.ApiService.Services;
using Dashy.Net.Shared.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.Services.AddControllers();
builder.AddServiceDefaults();
var cnnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__dashy");
builder.Services.AddNpgsql<AppDbContext>(cnnectionString);
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

// Conditionally enable authentication/authorization
using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var settingsProvider = scope.ServiceProvider.GetRequiredService<AuthenticationSettingsProvider>();
    var settings = await settingsProvider.GetSettingsAsync();
    if (settings is not null && settings.IsEnabled)
    {
        builder.Services.AddAuthentication(options =>
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
            

            //options.Events = new OpenIdConnectEvents
            //{
            //    OnAuthorizationCodeReceived = context =>
            //    {
            //        context.TokenEndpointRequest.ClientSecret = settings.ClientSecret;
            //        return Task.CompletedTask;
            //    }
            //};
        });
    }
}

var app = builder.Build();

// Initialize the one-time token service (generates first token and logs it)
var oneTimeTokenService = app.Services.GetRequiredService<OneTimeTokenService>();
// Token is generated in the constructor and logged automatically

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

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
