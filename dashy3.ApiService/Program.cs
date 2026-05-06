using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;
using dashy3.ApiService.Data;
using dashy3.ApiService.Dtos;
using dashy3.ApiService.Entities;
using dashy3.ApiService.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var runMigrationsInApiService = builder.Configuration.GetValue<bool?>("Database:RunMigrationsInApiService")
    ?? builder.Environment.IsDevelopment();

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["ApiKey"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = "X-Api-Key",
            Description = "API key for automation and integration scenarios."
        };
        document.Components.SecuritySchemes["UserId"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = "X-User-Id",
            Description = "User identity header for authenticated frontend sessions."
        };
        return Task.CompletedTask;
    });
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var desc = context.Description;
        var acceptsApiKey =
            (desc.HttpMethod == "GET" && desc.RelativePath == "api/dashboards") ||
            (desc.HttpMethod == "PUT" && desc.RelativePath == "api/widgets/{widgetId}/content");
        if (acceptsApiKey)
        {
            // Both X-Api-Key and X-User-Id are accepted; express as alternatives (OR).
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("ApiKey")] = []
                },
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("UserId")] = []
                }
            ];
        }
        return Task.CompletedTask;
    });
});

builder.Services.AddHttpClient("weather", c => { c.Timeout = TimeSpan.FromSeconds(10); });

builder.AddNpgsqlDbContext<DashboardDbContext>("dashy3db");

builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<DashboardDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddDataProtection();

builder.Services.AddHttpClient("rss",c =>
{
    c.Timeout = TimeSpan.FromSeconds(10);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; Dashy/1.0)");
});

builder.Services.AddHttpClient("nvd", c =>
{
    c.BaseAddress = new Uri("https://services.nvd.nist.gov/");
    c.Timeout = TimeSpan.FromSeconds(30);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; Dashy/1.0)");
});

builder.Services.AddHttpClient("euvd", c =>
{
    c.BaseAddress = new Uri("https://euvd.enisa.europa.eu/");
    c.Timeout = TimeSpan.FromSeconds(30);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; Dashy/1.0)");
});

var app = builder.Build();

app.MapScalarApiReference();

app.UseExceptionHandler();

app.MapOpenApi();
    

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DashboardDbContext>();
    var startupLogger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Startup");

    if (runMigrationsInApiService)
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        startupLogger.LogInformation(
            "Skipping API service database migration because Database:RunMigrationsInApiService is disabled.");
    }

    if (!await RolesTableExistsAsync(dbContext))
    {
        startupLogger.LogWarning(
            "Skipping role seeding because Identity schema is not ready (AspNetRoles table missing).");
    }
    else
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

static async Task<bool> RolesTableExistsAsync(DashboardDbContext dbContext)
{
    var conn = dbContext.Database.GetDbConnection();
    await conn.OpenAsync();
    try
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetRoles')";
        return (bool)(await cmd.ExecuteScalarAsync())!;
    }
    finally
    {
        await conn.CloseAsync();
    }
}

static WidgetDto ToWidgetDto(WidgetEntity w)
{
    Dictionary<string, JsonElement> config;
    try { config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(w.ConfigJson) ?? []; }
    catch { config = []; }
    return new WidgetDto(w.Id, w.Type, w.Title, w.Icon, w.Size, config, w.ParentWidgetId, w.Order);
}

static DashboardDto ToDashboardDto(DashboardEntity d, string? userPermission = null) =>
    new(d.Id, d.Name, d.LayoutMode,
        d.Widgets.OrderBy(w => w.Order).Select(ToWidgetDto).ToList(),
        d.OwnerId,
        d.Visibility,
        userPermission,
        d.AutoScroll,
        d.ScrollSpeed);

static async Task CreatePersonalDashboardAsync(DashboardDbContext db, string userId, string displayName)
{
    var count = await db.Dashboards.CountAsync();
    var dash = new DashboardEntity
    {
        Name = "Personal",
        LayoutMode = "Auto",
        Order = count,
        OwnerId = userId,
        Visibility = "Private",
        Widgets =
        [
            new WidgetEntity
            {
                Type    = "markdown",
                Title   = "Welcome",
                Icon    = "Smile",
                Size    = "Medium",
                Order   = 0,
                ConfigJson = $$"""{"content":"# Welcome, {{displayName}}! 👋\nThis is your personal dashboard. Add widgets using the **+** button to get started."}"""
            }
        ]
    };
    db.Dashboards.Add(dash);
    await db.SaveChangesAsync();
}

var api = app.MapGroup("/api");

static async Task<string?> GetCollaboratorPermissionAsync(DashboardDbContext db, string dashboardId, string? userId)
{
    if (string.IsNullOrEmpty(userId)) return null;
    var c = await db.Collaborators.FirstOrDefaultAsync(c => c.DashboardId == dashboardId && c.UserId == userId);
    return c?.Permission;
}

static async Task<bool> CanViewAsync(DashboardEntity dashboard, string? userId, DashboardDbContext db)
{
    if (dashboard.Visibility == "Shared") return true;
    if (!string.IsNullOrEmpty(userId) && dashboard.OwnerId == userId) return true;
    return await db.Collaborators.AnyAsync(c => c.DashboardId == dashboard.Id && c.UserId == userId);
}

static async Task<bool> CanEditAsync(DashboardEntity dashboard, string? userId, DashboardDbContext db)
{
    if (string.IsNullOrEmpty(userId)) return false;
    if (dashboard.OwnerId == userId) return true;
    var perm = await GetCollaboratorPermissionAsync(db, dashboard.Id, userId);
    return perm == "Editor";
}

static async Task<bool> CanShareAsync(DashboardEntity dashboard, string? userId, DashboardDbContext db)
    => await CanEditAsync(dashboard, userId, db);

api.MapGet("/dashboards", async (HttpRequest request, DashboardDbContext db) =>
{
    // API key auth path takes priority
    var rawApiKey = request.Headers["X-Api-Key"].FirstOrDefault();
    if (!string.IsNullOrWhiteSpace(rawApiKey))
    {
        var hash = ApiKeyHelper.ComputeHash(rawApiKey);
        var apiKey = await db.ApiKeys.FirstOrDefaultAsync(k => k.KeyHash == hash);

        if (apiKey == null || apiKey.IsRevoked ||
            (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow))
            return Results.Unauthorized();

        apiKey.LastUsedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        IQueryable<DashboardEntity> query = db.Dashboards.Include(d => d.Widgets);

        if (!apiKey.IsGlobal)
        {
            if (apiKey.AllowedDashboardIds != null)
            {
                var allowed = JsonSerializer.Deserialize<List<string>>(apiKey.AllowedDashboardIds) ?? [];
                query = query.Where(d => allowed.Contains(d.Id));
            }
            else
            {
                var ownerId = apiKey.OwnerId;
                query = query.Where(d => d.Visibility == "Shared" || d.OwnerId == ownerId
                    || db.Collaborators.Any(c => c.DashboardId == d.Id && c.UserId == ownerId));
            }
        }

        var apiKeyDashboards = await query.OrderBy(d => d.Order).ToListAsync();
        return Results.Ok(apiKeyDashboards.Select(d => ToDashboardDto(d, null)));
    }

    // Existing X-User-Id auth path
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var dashboards = await db.Dashboards
        .Include(d => d.Widgets)
        .Where(d => d.Visibility == "Shared" || d.OwnerId == userId
            || db.Collaborators.Any(c => c.DashboardId == d.Id && c.UserId == userId))
        .OrderBy(d => d.Order)
        .ToListAsync();

    var collabPerms = string.IsNullOrEmpty(userId)
        ? []
        : await db.Collaborators
            .Where(c => c.UserId == userId)
            .ToDictionaryAsync(c => c.DashboardId, c => c.Permission);

    return Results.Ok(dashboards.Select(d =>
    {
        string? perm = null;
        if (!string.IsNullOrEmpty(userId))
        {
            if (d.OwnerId == userId) perm = "Owner";
            else collabPerms.TryGetValue(d.Id, out perm);
        }
        return ToDashboardDto(d, perm);
    }));
});

api.MapGet("/dashboards/{id}", async (string id, HttpRequest request, DashboardDbContext db) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var d = await db.Dashboards
        .Include(d => d.Widgets)
        .FirstOrDefaultAsync(d => d.Id == id);
    if (d is null) return Results.NotFound();
    if (!await CanViewAsync(d, userId, db)) return Results.StatusCode(403);

    string? perm = null;
    if (!string.IsNullOrEmpty(userId))
    {
        if (d.OwnerId == userId) perm = "Owner";
        else perm = await GetCollaboratorPermissionAsync(db, id, userId);
    }
    return Results.Ok(ToDashboardDto(d, perm));
});

api.MapPost("/dashboards", async (CreateDashboardRequest req, HttpRequest request, DashboardDbContext db) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var count = await db.Dashboards.CountAsync();
    var entity = new DashboardEntity
    {
        Name = req.Name,
        LayoutMode = req.LayoutMode,
        Order = count,
        OwnerId = userId,
        Visibility = req.Visibility
    };
    db.Dashboards.Add(entity);
    await db.SaveChangesAsync();
    return Results.Created($"/api/dashboards/{entity.Id}", ToDashboardDto(entity, "Owner"));
});

api.MapPut("/dashboards/{id}", async (string id, UpdateDashboardRequest req, HttpRequest request, DashboardDbContext db) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var entity = await db.Dashboards.FindAsync(id);
    if (entity is null) return Results.NotFound();
    if (entity.OwnerId != userId)
        return Results.StatusCode(403);
    entity.Name = req.Name;
    entity.LayoutMode = req.LayoutMode;
    entity.AutoScroll = req.AutoScroll;
    entity.ScrollSpeed = req.ScrollSpeed;
    await db.SaveChangesAsync();
    return Results.Ok(new DashboardDto(entity.Id, entity.Name, entity.LayoutMode, [], entity.OwnerId, entity.Visibility, "Owner", entity.AutoScroll, entity.ScrollSpeed));
});

api.MapDelete("/dashboards/{id}", async (string id, HttpRequest request, DashboardDbContext db) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var entity = await db.Dashboards.FindAsync(id);
    if (entity is null) return Results.NotFound();
    if (entity.OwnerId != userId)
        return Results.StatusCode(403);
    db.Dashboards.Remove(entity);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapPatch("/dashboards/{id}/visibility", async (string id, UpdateDashboardVisibilityRequest req, HttpRequest request, DashboardDbContext db) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var entity = await db.Dashboards.FindAsync(id);
    if (entity is null) return Results.NotFound();
    if (entity.OwnerId != userId)
        return Results.StatusCode(403);
    if (req.Visibility != "Shared" && req.Visibility != "Private")
        return Results.BadRequest(new { error = "Visibility must be 'Shared' or 'Private'" });
    entity.Visibility = req.Visibility;
    await db.SaveChangesAsync();
    return Results.Ok(new { id = entity.Id, visibility = entity.Visibility });
});

api.MapPost("/dashboards/{dashboardId}/widgets", async (string dashboardId, CreateWidgetRequest req, HttpRequest request, DashboardDbContext db) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var dashboard = await db.Dashboards.Include(d => d.Widgets).FirstOrDefaultAsync(d => d.Id == dashboardId);
    if (dashboard is null) return Results.NotFound();
    if (!await CanEditAsync(dashboard, userId, db)) return Results.StatusCode(403);

        if (req.ParentWidgetId is not null)
    {
        var parent = await db.Widgets.FirstOrDefaultAsync(w => w.Id == req.ParentWidgetId && w.DashboardId == dashboardId);
        if (parent is null) return Results.BadRequest(new { error = "Parent widget not found" });
        if (parent.Type != "section") return Results.BadRequest(new { error = "Parent must be a section widget" });
        if (req.Type == "section" && parent.ParentWidgetId is not null)
            return Results.BadRequest(new { error = "Cannot nest sections more than one level deep" });
    }

    var entity = new WidgetEntity
    {
        DashboardId = dashboardId,
        Type = req.Type,
        Title = req.Title,
        Icon = req.Icon,
        Size = req.Size,
        ParentWidgetId = req.ParentWidgetId,
        Order = dashboard.Widgets.Count,
        ConfigJson = req.Config is not null ? JsonSerializer.Serialize(req.Config) : "{}"
    };
    db.Widgets.Add(entity);
    await db.SaveChangesAsync();
    return Results.Created($"/api/dashboards/{dashboardId}/widgets/{entity.Id}", ToWidgetDto(entity));
});

api.MapPut("/dashboards/{dashboardId}/widgets/{widgetId}", async (string dashboardId, string widgetId, UpdateWidgetRequest req, HttpRequest request, DashboardDbContext db) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var dashboard = await db.Dashboards.FirstOrDefaultAsync(d => d.Id == dashboardId);
    if (dashboard is null) return Results.NotFound();
    if (!await CanEditAsync(dashboard, userId, db)) return Results.StatusCode(403);

    var entity = await db.Widgets.FirstOrDefaultAsync(w => w.Id == widgetId && w.DashboardId == dashboardId);
    if (entity is null) return Results.NotFound();

        if (req.ParentWidgetId is not null)
        {
            var parent = await db.Widgets.FirstOrDefaultAsync(w => w.Id == req.ParentWidgetId && w.DashboardId == dashboardId);
            if (parent is null) return Results.BadRequest(new { error = "Parent widget not found" });
            if (parent.Type != "section") return Results.BadRequest(new { error = "Parent must be a section widget" });
            if (entity.Type == "section" && parent.ParentWidgetId is not null)
            return Results.BadRequest(new { error = "Cannot nest sections more than one level deep" });
    }

    entity.Title = req.Title;
    entity.Icon = req.Icon;
    entity.Size = req.Size;
    entity.ParentWidgetId = req.ParentWidgetId;
    entity.Order = req.Order;
    entity.ConfigJson = req.Config is not null ? JsonSerializer.Serialize(req.Config) : entity.ConfigJson;
    await db.SaveChangesAsync();
    return Results.Ok(ToWidgetDto(entity));
});

api.MapDelete("/dashboards/{dashboardId}/widgets/{widgetId}", async (string dashboardId, string widgetId, HttpRequest request, DashboardDbContext db) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var dashboard = await db.Dashboards.FirstOrDefaultAsync(d => d.Id == dashboardId);
    if (dashboard is null) return Results.NotFound();
    if (!await CanEditAsync(dashboard, userId, db)) return Results.StatusCode(403);

    var entity = await db.Widgets.FirstOrDefaultAsync(w => w.Id == widgetId && w.DashboardId == dashboardId);
    if (entity is null) return Results.NotFound();
    db.Widgets.Remove(entity);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapPut("/widgets/{widgetId}/content", async (string widgetId, UpdateWidgetContentRequest req, HttpRequest request, DashboardDbContext db) =>
{
    if ((req.Content?.Length ?? 0) > 100_000)
        return Results.BadRequest(new { error = "content exceeds maximum length of 100,000 characters" });
    if ((req.Css?.Length ?? 0) > 20_000)
        return Results.BadRequest(new { error = "css exceeds maximum length of 20,000 characters" });

    var entity = await db.Widgets.Include(w => w.Dashboard).FirstOrDefaultAsync(w => w.Id == widgetId);
    if (entity is null) return Results.NotFound();
    var dashboard = entity.Dashboard;

    var rawApiKey = request.Headers["X-Api-Key"].FirstOrDefault();
    if (rawApiKey is not null)
    {
        var keyHash = ApiKeyHelper.ComputeHash(rawApiKey);
        var apiKey = await db.ApiKeys.FirstOrDefaultAsync(k => k.KeyHash == keyHash);
        if (apiKey is null || apiKey.IsRevoked || (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow))
            return Results.StatusCode(401);

        apiKey.LastUsedAt = DateTime.UtcNow;

        if (!apiKey.IsGlobal)
        {
            if (apiKey.AllowedDashboardIds is not null)
            {
                var allowedIds = JsonSerializer.Deserialize<List<string>>(apiKey.AllowedDashboardIds) ?? [];
                if (!allowedIds.Contains(dashboard.Id))
                    return Results.StatusCode(403);
            }
            else
            {
                var isOwner = dashboard.OwnerId == apiKey.OwnerId;
                var isEditor = !isOwner && await db.Collaborators.AnyAsync(
                    c => c.DashboardId == dashboard.Id && c.UserId == apiKey.OwnerId && c.Permission == "Editor");
                if (!isOwner && !isEditor)
                    return Results.StatusCode(403);
            }
        }
    }
    else
    {
        var userId = request.Headers["X-User-Id"].FirstOrDefault();
        if (!await CanEditAsync(dashboard, userId, db))
            return Results.StatusCode(403);
    }

    var config = new Dictionary<string, JsonElement>();
    try
    {
        var existing = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(entity.ConfigJson);
        if (existing is not null)
            config = existing;
    }
    catch { /* start with empty dict */ }

    config["content"] = JsonSerializer.SerializeToElement(req.Content ?? "");
    config["css"] = JsonSerializer.SerializeToElement(req.Css ?? "");
    entity.ConfigJson = JsonSerializer.Serialize(config);

    await db.SaveChangesAsync();
    return Results.Ok(new { widgetId = entity.Id, updatedAt = DateTime.UtcNow });
});

api.MapGet("/dashboards/{id}/collaborators", async (string id, HttpRequest request, DashboardDbContext db, UserManager<AppUser> userManager) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var dashboard = await db.Dashboards.FindAsync(id);
    if (dashboard is null) return Results.NotFound();
    if (!await CanViewAsync(dashboard, userId, db)) return Results.StatusCode(403);

    var collaborators = await db.Collaborators
        .Where(c => c.DashboardId == id)
        .ToListAsync();

    var result = new List<CollaboratorDto>();

        var owner = await userManager.FindByIdAsync(dashboard.OwnerId ?? "");
    if (owner is not null)
        result.Add(new CollaboratorDto(owner.Id, owner.Email ?? "", owner.DisplayName, owner.AvatarUrl, "Owner"));

    foreach (var c in collaborators)
    {
        var user = await userManager.FindByIdAsync(c.UserId);
        if (user is null) continue;
        result.Add(new CollaboratorDto(c.UserId, user.Email ?? "", user.DisplayName, user.AvatarUrl, c.Permission));
    }
    return Results.Ok(result);
});

api.MapPost("/dashboards/{id}/collaborators", async (string id, AddCollaboratorRequest req, HttpRequest request, DashboardDbContext db, UserManager<AppUser> userManager) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var dashboard = await db.Dashboards.FindAsync(id);
    if (dashboard is null) return Results.NotFound();
    if (!await CanShareAsync(dashboard, userId, db)) return Results.StatusCode(403);

    if (req.Permission != "Viewer" && req.Permission != "Editor")
        return Results.BadRequest(new { error = "Permission must be 'Viewer' or 'Editor'" });
    if (req.UserId == dashboard.OwnerId)
        return Results.BadRequest(new { error = "Cannot add the dashboard owner as a collaborator" });

    var targetUser = await userManager.FindByIdAsync(req.UserId);
    if (targetUser is null) return Results.BadRequest(new { error = "User not found" });

    var existing = await db.Collaborators.FindAsync(id, req.UserId);
    if (existing is not null)
        return Results.Conflict(new { error = "User is already a collaborator" });

    db.Collaborators.Add(new DashboardCollaboratorEntity
    {
        DashboardId = id,
        UserId = req.UserId,
        Permission = req.Permission
    });
    await db.SaveChangesAsync();
    return Results.Created($"/api/dashboards/{id}/collaborators/{req.UserId}",
        new CollaboratorDto(targetUser.Id, targetUser.Email ?? "", targetUser.DisplayName, targetUser.AvatarUrl, req.Permission));
});

api.MapPut("/dashboards/{id}/collaborators/{targetUserId}", async (string id, string targetUserId, UpdateCollaboratorRequest req, HttpRequest request, DashboardDbContext db) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var dashboard = await db.Dashboards.FindAsync(id);
    if (dashboard is null) return Results.NotFound();
    if (!await CanShareAsync(dashboard, userId, db)) return Results.StatusCode(403);

    if (req.Permission != "Viewer" && req.Permission != "Editor")
        return Results.BadRequest(new { error = "Permission must be 'Viewer' or 'Editor'" });

    var collab = await db.Collaborators.FindAsync(id, targetUserId);
    if (collab is null) return Results.NotFound();

    collab.Permission = req.Permission;
    await db.SaveChangesAsync();
    return Results.Ok(new { dashboardId = id, userId = targetUserId, permission = req.Permission });
});

api.MapDelete("/dashboards/{id}/collaborators/{targetUserId}", async (string id, string targetUserId, HttpRequest request, DashboardDbContext db) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    var dashboard = await db.Dashboards.FindAsync(id);
    if (dashboard is null) return Results.NotFound();

        var isOwner = dashboard.OwnerId == userId;
        if (!isOwner)
        {
            var callerPerm = await GetCollaboratorPermissionAsync(db, id, userId);
            if (callerPerm != "Editor") return Results.StatusCode(403);
            var targetPerm = await GetCollaboratorPermissionAsync(db, id, targetUserId);
            if (targetPerm != "Viewer") return Results.StatusCode(403);
        }

    var collab = await db.Collaborators.FindAsync(id, targetUserId);
    if (collab is null) return Results.NotFound();
    db.Collaborators.Remove(collab);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapGet("/users/search", async (string? q, HttpRequest request, UserManager<AppUser> userManager) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

    var query = (q ?? "").Trim().ToLower();
    var users = userManager.Users
        .Where(u => u.Id != userId &&
            (string.IsNullOrEmpty(query) ||
             (u.Email != null && u.Email.ToLower().Contains(query)) ||
             u.DisplayName.ToLower().Contains(query)))
        .Take(10)
        .Select(u => new { u.Id, u.Email, u.DisplayName, u.AvatarUrl })
        .ToList();
    return Results.Ok(users);
});

api.MapGet("/stats", () =>
{
    var gcInfo = GC.GetGCMemoryInfo();
    long totalMemBytes = gcInfo.TotalAvailableMemoryBytes > 0 ? gcInfo.TotalAvailableMemoryBytes : 8L * 1024 * 1024 * 1024;
    long usedMemBytes = Environment.WorkingSet;
    double memPercent = totalMemBytes > 0 ? (usedMemBytes * 100.0 / totalMemBytes) : 0;

    int processCount = 0;
    try { processCount = System.Diagnostics.Process.GetProcesses().Length; } catch { }

    return Results.Ok(new
    {
        memoryUsedMb = Math.Round(usedMemBytes / (1024.0 * 1024.0), 1),
        memoryTotalMb = Math.Round(totalMemBytes / (1024.0 * 1024.0), 1),
        memoryPercent = Math.Round(memPercent, 1),
        uptimeHours = Math.Round(Environment.TickCount64 / 3_600_000.0, 1),
        processCount,
        processorCount = Environment.ProcessorCount
    });
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

api.MapGet("/weather", async (string location, string? unit, IHttpClientFactory factory) =>
{
    var http = factory.CreateClient("weather");

        GeoResult[]? geoResults = null;
    try
    {
        var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(location)}&count=1&language=en&format=json";
        var geoResponse = await http.GetFromJsonAsync<GeoResponse>(geoUrl);
        geoResults = geoResponse?.Results;
    }
    catch { return Results.NotFound(new { error = "Location not found" }); }

    if (geoResults is null || geoResults.Length == 0)
        return Results.NotFound(new { error = "Location not found" });

    var geo = geoResults[0];
    var locationName = string.IsNullOrEmpty(geo.Country)
        ? geo.Name
        : $"{geo.Name}, {geo.Country}";

            WeatherCurrent? current;
    try
    {
        var lat = geo.Latitude.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
        var lon = geo.Longitude.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
        var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,relative_humidity_2m,wind_speed_10m,cloud_cover,weather_code&wind_speed_unit=kmh";
        var weatherResponse = await http.GetFromJsonAsync<WeatherResponse>(weatherUrl);
        current = weatherResponse?.Current;
    }
    catch { return Results.Problem("Failed to fetch weather data"); }

    if (current is null)
        return Results.Problem("No weather data returned");

    var tempC = current.Temperature2m;
    var isF = string.Equals(unit, "fahrenheit", StringComparison.OrdinalIgnoreCase);
    var temperature = isF ? tempC * 9.0 / 5.0 + 32 : tempC;

    var description = current.WeatherCode switch
    {
        0 => "Clear Sky",
        1 or 2 or 3 => "Partly Cloudy",
        45 or 48 => "Foggy",
        >= 51 and <= 67 => "Rainy",
        >= 71 and <= 77 => "Snowy",
        >= 80 and <= 82 => "Rain Showers",
        >= 95 => "Thunderstorm",
        _ => "Unknown"
    };

    return Results.Ok(new
    {
        location = locationName,
        temperature = Math.Round(temperature, 1),
        temperatureUnit = isF ? "F" : "C",
        description,
        humidity = current.RelativeHumidity2m,
        windSpeed = current.WindSpeed10m,
        cloudCover = current.CloudCover,
        weatherCode = current.WeatherCode
    });
});

app.MapDefaultEndpoints();

var settingsApi = api.MapGroup("/settings");

settingsApi.MapGet("/oidc", async (DashboardDbContext db) =>
{
    var config = await db.OidcConfig.FindAsync(1);
    if (config is null)
    {
        config = new OidcConfigEntity { Id = 1 };
        db.OidcConfig.Add(config);
        await db.SaveChangesAsync();
    }
    return Results.Ok(new OidcConfigDto(
        config.Authority,
        config.ClientId,
        "",
        config.Scopes,
        config.CallbackPath,
        config.IsEnabled,
        config.AllowAutoRegistration,
        config.UpdatedAt));
});

settingsApi.MapGet("/oidc/startup", async (DashboardDbContext db) =>
{
    var config = await db.OidcConfig.FindAsync(1);
    if (config is null)
        return Results.Ok(new OidcStartupDto("", "", "", "", "", false, true));

    return Results.Ok(new OidcStartupDto(
        config.Authority,
        config.ClientId,
        config.ClientSecret,
        config.Scopes,
        config.CallbackPath,
        config.IsEnabled,
        config.AllowAutoRegistration));
});

settingsApi.MapPut("/oidc", async (UpdateOidcConfigRequest req, DashboardDbContext db) =>
{
    var config = await db.OidcConfig.FindAsync(1);
    if (config is null)
    {
        config = new OidcConfigEntity { Id = 1 };
        db.OidcConfig.Add(config);
    }

    config.Authority = req.Authority;
    config.ClientId = req.ClientId;
    if (!string.IsNullOrEmpty(req.ClientSecret))
        config.ClientSecret = req.ClientSecret;
    config.Scopes = req.Scopes;
    config.CallbackPath = req.CallbackPath;
    config.IsEnabled = req.IsEnabled;
    config.AllowAutoRegistration = req.AllowAutoRegistration;
    config.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(new OidcConfigDto(
        config.Authority,
        config.ClientId,
        "",
        config.Scopes,
        config.CallbackPath,
        config.IsEnabled,
        config.AllowAutoRegistration,
        config.UpdatedAt));
});

api.MapGet("/rss", async (string url, int? limit, IHttpClientFactory factory) =>
{
    if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        return Results.BadRequest(new { error = "Invalid URL" });

    var client = factory.CreateClient("rss");
    try
    {
        var xml = await client.GetStringAsync(url);
        var doc = XDocument.Parse(xml);
        XNamespace atom = "http://www.w3.org/2005/Atom";

        var maxItems = Math.Min(limit ?? 10, 20);

        var rssItems = doc.Descendants("item").Take(maxItems).Select(item => (object)new
        {
            title = item.Element("title")?.Value ?? "",
            link = item.Element("link")?.Value ?? "",
            description = StripHtml(item.Element("description")?.Value ?? ""),
            pubDate = item.Element("pubDate")?.Value ?? ""
        }).ToList();

        if (rssItems.Count == 0)
        {
            rssItems = doc.Descendants(atom + "entry").Take(maxItems).Select(entry => (object)new
            {
                title = entry.Element(atom + "title")?.Value ?? "",
                link = entry.Elements(atom + "link").FirstOrDefault()?.Attribute("href")?.Value ?? "",
                description = StripHtml(entry.Element(atom + "summary")?.Value ?? entry.Element(atom + "content")?.Value ?? ""),
                pubDate = entry.Element(atom + "updated")?.Value ?? ""
            }).ToList();
        }

        var feedTitle = doc.Descendants("channel").FirstOrDefault()?.Element("title")?.Value
                     ?? doc.Descendants(atom + "title").FirstOrDefault()?.Value
                     ?? "RSS Feed";

        return Results.Ok(new { title = feedTitle, items = rssItems });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to fetch feed: {ex.Message}");
    }
});

static string StripHtml(string html)
{
    if (string.IsNullOrWhiteSpace(html)) return "";
    var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
    text = System.Net.WebUtility.HtmlDecode(text);
    return text.Trim().Length > 200 ? text.Trim()[..200] + "..." : text.Trim();
}

var authApi = api.MapGroup("/auth");

authApi.MapGet("/check-setup", async (UserManager<AppUser> userManager) =>
{
    var count = await userManager.Users.CountAsync();
    return Results.Ok(new { needsSetup = count == 0 });
});

authApi.MapPost("/register", async (RegisterRequest req, UserManager<AppUser> userManager, DashboardDbContext db) =>
{
    var existing = await userManager.FindByEmailAsync(req.Email);
    if (existing is not null)
        return Results.Conflict(new { error = "Email already registered" });

    var user = new AppUser
    {
        UserName = req.Email,
        Email = req.Email,
        DisplayName = req.DisplayName,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(user, req.Password);
    if (!result.Succeeded)
        return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });

    var role = req.Role == "Admin" ? "Admin" : "User";
    await userManager.AddToRoleAsync(user, role);
    await CreatePersonalDashboardAsync(db, user.Id, user.DisplayName);

    var roles = await userManager.GetRolesAsync(user);
    return Results.Created($"/api/auth/me", new UserDto(user.Id, user.Email!, user.DisplayName, user.AvatarUrl, roles.ToArray(), user.CreatedAt, user.LastLoginAt));
});

authApi.MapPost("/login", async (LoginRequest req, UserManager<AppUser> userManager) =>
{
    var user = await userManager.FindByEmailAsync(req.Email);
    if (user is null)
        return Results.Unauthorized();

    var valid = await userManager.CheckPasswordAsync(user, req.Password);
    if (!valid)
        return Results.Unauthorized();

    user.LastLoginAt = DateTime.UtcNow;
    await userManager.UpdateAsync(user);

    var roles = await userManager.GetRolesAsync(user);
    return Results.Ok(new UserDto(user.Id, user.Email!, user.DisplayName, user.AvatarUrl, roles.ToArray(), user.CreatedAt, user.LastLoginAt));
});

authApi.MapGet("/me", async (HttpRequest request, UserManager<AppUser> userManager) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

    var user = await userManager.FindByIdAsync(userId);
    if (user is null) return Results.NotFound();

    var roles = await userManager.GetRolesAsync(user);
    return Results.Ok(new UserDto(user.Id, user.Email!, user.DisplayName, user.AvatarUrl, roles.ToArray(), user.CreatedAt, user.LastLoginAt));
});

authApi.MapPut("/profile", async (UpdateProfileRequest req, HttpRequest request, UserManager<AppUser> userManager) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

    var user = await userManager.FindByIdAsync(userId);
    if (user is null) return Results.NotFound();

    user.DisplayName = req.DisplayName;
    user.AvatarUrl = req.AvatarUrl;
    await userManager.UpdateAsync(user);

    var roles = await userManager.GetRolesAsync(user);
    return Results.Ok(new UserDto(user.Id, user.Email!, user.DisplayName, user.AvatarUrl, roles.ToArray(), user.CreatedAt, user.LastLoginAt));
});

authApi.MapPost("/change-password", async (ChangePasswordRequest req, UserManager<AppUser> userManager) =>
{
    var user = await userManager.FindByIdAsync(req.UserId);
    if (user is null) return Results.NotFound();

    var result = await userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
    if (!result.Succeeded)
        return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });

    return Results.Ok(new { message = "Password changed successfully" });
});

authApi.MapPost("/oidc-login", async (OidcLoginRequest req, DashboardDbContext db, UserManager<AppUser> userManager) =>
{
    if (string.IsNullOrWhiteSpace(req.Sub))
        return Results.BadRequest(new { error = "Sub is required for OIDC login" });

    var email = string.IsNullOrWhiteSpace(req.Email)
        ? $"oidc_{req.Sub.GetHashCode():x}@oidc.local"
        : req.Email;

    var user = db.Users.FirstOrDefault(u => u.OidcSub == req.Sub);
    user ??= await userManager.FindByEmailAsync(email);

    if (user is null)
    {
        // Check if auto-registration is allowed
        var oidcConfig = await db.OidcConfig.FindAsync(1);
        if (oidcConfig is not null && !oidcConfig.AllowAutoRegistration)
        {
            // Check if there's a pending invite for this email
            var invite = await db.Invites.FirstOrDefaultAsync(i => 
                i.Email.ToLower() == email.ToLower() && 
                !i.IsUsed && 
                i.ExpiresAt > DateTime.UtcNow);

            if (invite is null)
            {
                return Results.Json(new { 
                    error = "registration_disabled", 
                    message = "Auto-registration is disabled. Please contact an administrator to receive an invite." 
                }, statusCode: 403);
            }

            // Mark invite as used
            invite.IsUsed = true;
            invite.UsedAt = DateTime.UtcNow;
        }

        user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = req.Name ?? email,
            AvatarUrl = req.AvatarUrl,
            OidcSub = req.Sub,
            EmailConfirmed = true
        };
        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            return Results.BadRequest(new { errors = createResult.Errors.Select(e => e.Description) });

        // Check if invite specified a role, otherwise default to User
        var inviteForRole = await db.Invites.FirstOrDefaultAsync(i => 
            i.Email.ToLower() == email.ToLower() && 
            i.IsUsed && 
            i.AcceptedByUserId == null);
        var role = inviteForRole?.Role ?? "User";
        await userManager.AddToRoleAsync(user, role);

        if (inviteForRole is not null)
        {
            inviteForRole.AcceptedByUserId = user.Id;
            await db.SaveChangesAsync();
        }

        await CreatePersonalDashboardAsync(db, user.Id, user.DisplayName);
    }
    else
    {
        user.OidcSub ??= req.Sub;
        if (!string.IsNullOrEmpty(req.Name) && user.DisplayName != req.Name) user.DisplayName = req.Name;
        if (req.AvatarUrl is not null && user.AvatarUrl != req.AvatarUrl) user.AvatarUrl = req.AvatarUrl;
    }

    user.LastLoginAt = DateTime.UtcNow;
    await userManager.UpdateAsync(user);

    var roles = await userManager.GetRolesAsync(user);
    return Results.Ok(new UserDto(user.Id, user.Email!, user.DisplayName, user.AvatarUrl, roles.ToArray(), user.CreatedAt, user.LastLoginAt));
});

var adminApi = api.MapGroup("/admin");

adminApi.MapGet("/users", async (UserManager<AppUser> userManager) =>
{
    var users = userManager.Users.ToList();
    var result = new List<UserDto>();
    foreach (var u in users)
    {
        var roles = await userManager.GetRolesAsync(u);
        result.Add(new UserDto(u.Id, u.Email!, u.DisplayName, u.AvatarUrl, roles.ToArray(), u.CreatedAt, u.LastLoginAt));
    }
    return Results.Ok(result);
});

adminApi.MapPut("/users/{id}/role", async (string id, UpdateUserRoleRequest req, UserManager<AppUser> userManager) =>
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null) return Results.NotFound();

    if (req.Action == "add")
        await userManager.AddToRoleAsync(user, req.Role);
    else if (req.Action == "remove")
        await userManager.RemoveFromRoleAsync(user, req.Role);
    else
        return Results.BadRequest(new { error = "Action must be 'add' or 'remove'" });

    var roles = await userManager.GetRolesAsync(user);
    return Results.Ok(new UserDto(user.Id, user.Email!, user.DisplayName, user.AvatarUrl, roles.ToArray(), user.CreatedAt, user.LastLoginAt));
});

adminApi.MapDelete("/users/{id}", async (string id, HttpRequest request, UserManager<AppUser> userManager) =>
{
    var requesterId = request.Headers["X-User-Id"].FirstOrDefault();
    if (id == requesterId)
        return Results.BadRequest(new { error = "Cannot delete your own account" });

    var user = await userManager.FindByIdAsync(id);
    if (user is null) return Results.NotFound();

    await userManager.DeleteAsync(user);
    return Results.NoContent();
});

adminApi.MapPost("/users/invite", async (RegisterRequest req, UserManager<AppUser> userManager, DashboardDbContext db) =>
{
    var existing = await userManager.FindByEmailAsync(req.Email);
    if (existing is not null)
        return Results.Conflict(new { error = "Email already registered" });

    // Check if there's already a pending invite for this email
    var existingInvite = await db.Invites.FirstOrDefaultAsync(i => 
        i.Email.ToLower() == req.Email.ToLower() && 
        !i.IsUsed && 
        i.ExpiresAt > DateTime.UtcNow);
    if (existingInvite is not null)
        return Results.Conflict(new { error = "An invite is already pending for this email" });

    // If password is provided, create user directly (legacy behavior)
    // Otherwise, create an invite token
    if (!string.IsNullOrEmpty(req.Password))
    {
        var user = new AppUser
        {
            UserName = req.Email,
            Email = req.Email,
            DisplayName = req.DisplayName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        var role = req.Role == "Admin" ? "Admin" : "User";
        await userManager.AddToRoleAsync(user, role);
        await CreatePersonalDashboardAsync(db, user.Id, user.DisplayName);

        var roles = await userManager.GetRolesAsync(user);
        return Results.Created($"/api/auth/me", new UserDto(user.Id, user.Email!, user.DisplayName, user.AvatarUrl, roles.ToArray(), user.CreatedAt, user.LastLoginAt));
    }
    else
    {
        // Create invite token
        var invite = new InviteEntity
        {
            Email = req.Email,
            Role = req.Role == "Admin" ? "Admin" : "User"
        };
        db.Invites.Add(invite);
        await db.SaveChangesAsync();

        return Results.Created($"/api/admin/invites/{invite.Id}", new InviteDto(
            invite.Id,
            invite.Token,
            invite.Email,
            invite.Role,
            invite.InvitedByUserId,
            invite.CreatedAt,
            invite.ExpiresAt,
            invite.IsUsed,
            invite.UsedAt));
    }
});

// ── Invite Management ──────────────────────────────────────────────────────────

adminApi.MapGet("/invites", async (DashboardDbContext db) =>
{
    var invites = await db.Invites
        .OrderByDescending(i => i.CreatedAt)
        .ToListAsync();

    return Results.Ok(invites.Select(i => new InviteDto(
        i.Id,
        i.Token,
        i.Email,
        i.Role,
        i.InvitedByUserId,
        i.CreatedAt,
        i.ExpiresAt,
        i.IsUsed,
        i.UsedAt)));
});

adminApi.MapPost("/invites", async (HttpRequest request, CreateInviteRequest req, DashboardDbContext db, UserManager<AppUser> userManager) =>
{
    var inviterId = request.Headers["X-User-Id"].FirstOrDefault();

    var existing = await userManager.FindByEmailAsync(req.Email);
    if (existing is not null)
        return Results.Conflict(new { error = "Email already registered" });

    var existingInvite = await db.Invites.FirstOrDefaultAsync(i => 
        i.Email.ToLower() == req.Email.ToLower() && 
        !i.IsUsed && 
        i.ExpiresAt > DateTime.UtcNow);
    if (existingInvite is not null)
        return Results.Conflict(new { error = "An invite is already pending for this email" });

    var invite = new InviteEntity
    {
        Email = req.Email,
        Role = req.Role == "Admin" ? "Admin" : "User",
        InvitedByUserId = inviterId
    };
    db.Invites.Add(invite);
    await db.SaveChangesAsync();

    return Results.Created($"/api/admin/invites/{invite.Id}", new InviteDto(
        invite.Id,
        invite.Token,
        invite.Email,
        invite.Role,
        invite.InvitedByUserId,
        invite.CreatedAt,
        invite.ExpiresAt,
        invite.IsUsed,
        invite.UsedAt));
});

adminApi.MapDelete("/invites/{id}", async (string id, DashboardDbContext db) =>
{
    var invite = await db.Invites.FindAsync(id);
    if (invite is null) return Results.NotFound();

    db.Invites.Remove(invite);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

adminApi.MapPost("/invites/{id}/resend", async (string id, DashboardDbContext db) =>
{
    var invite = await db.Invites.FindAsync(id);
    if (invite is null) return Results.NotFound();
    if (invite.IsUsed) return Results.BadRequest(new { error = "Invite has already been used" });

    // Extend expiration
    invite.ExpiresAt = DateTime.UtcNow.AddDays(7);
    await db.SaveChangesAsync();

    // TODO: Send email if email service is configured

    return Results.Ok(new InviteDto(
        invite.Id,
        invite.Token,
        invite.Email,
        invite.Role,
        invite.InvitedByUserId,
        invite.CreatedAt,
        invite.ExpiresAt,
        invite.IsUsed,
        invite.UsedAt));
});

// ── Email Configuration ────────────────────────────────────────────────────────

settingsApi.MapGet("/email", async (DashboardDbContext db) =>
{
    var config = await db.EmailConfig.FindAsync(1);
    if (config is null)
    {
        config = new EmailConfigEntity { Id = 1 };
        db.EmailConfig.Add(config);
        await db.SaveChangesAsync();
    }
    return Results.Ok(new EmailConfigDto(
        config.Provider,
        config.IsEnabled,
        config.SmtpHost,
        config.SmtpPort,
        config.SmtpUseSsl,
        config.SmtpUsername,
        config.SmtpFromEmail,
        config.SmtpFromName,
        config.GraphTenantId,
        config.GraphClientId,
        config.GraphFromEmail,
        config.UpdatedAt));
});

settingsApi.MapPut("/email", async (UpdateEmailConfigRequest req, DashboardDbContext db) =>
{
    var config = await db.EmailConfig.FindAsync(1);
    if (config is null)
    {
        config = new EmailConfigEntity { Id = 1 };
        db.EmailConfig.Add(config);
    }

    config.Provider = req.Provider;
    config.IsEnabled = req.IsEnabled;

    // SMTP settings
    if (req.SmtpHost is not null) config.SmtpHost = req.SmtpHost;
    if (req.SmtpPort.HasValue) config.SmtpPort = req.SmtpPort.Value;
    if (req.SmtpUseSsl.HasValue) config.SmtpUseSsl = req.SmtpUseSsl.Value;
    if (req.SmtpUsername is not null) config.SmtpUsername = req.SmtpUsername;
    if (!string.IsNullOrEmpty(req.SmtpPassword)) config.SmtpPassword = req.SmtpPassword;
    if (req.SmtpFromEmail is not null) config.SmtpFromEmail = req.SmtpFromEmail;
    if (req.SmtpFromName is not null) config.SmtpFromName = req.SmtpFromName;

    // Graph settings
    if (req.GraphTenantId is not null) config.GraphTenantId = req.GraphTenantId;
    if (req.GraphClientId is not null) config.GraphClientId = req.GraphClientId;
    if (!string.IsNullOrEmpty(req.GraphClientSecret)) config.GraphClientSecret = req.GraphClientSecret;
    if (req.GraphFromEmail is not null) config.GraphFromEmail = req.GraphFromEmail;

    config.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(new EmailConfigDto(
        config.Provider,
        config.IsEnabled,
        config.SmtpHost,
        config.SmtpPort,
        config.SmtpUseSsl,
        config.SmtpUsername,
        config.SmtpFromEmail,
        config.SmtpFromName,
        config.GraphTenantId,
        config.GraphClientId,
        config.GraphFromEmail,
        config.UpdatedAt));
});

// ── Public Invite Acceptance ───────────────────────────────────────────────────

authApi.MapGet("/invite/{token}", async (string token, DashboardDbContext db) =>
{
    var invite = await db.Invites.FirstOrDefaultAsync(i => i.Token == token);
    if (invite is null)
        return Results.NotFound(new { error = "Invite not found" });

    var isValid = !invite.IsUsed && invite.ExpiresAt > DateTime.UtcNow;
    return Results.Ok(new InviteInfoDto(
        invite.Email,
        invite.Role,
        invite.ExpiresAt,
        isValid));
});

authApi.MapPost("/invite/{token}/accept", async (string token, AcceptInviteRequest req, DashboardDbContext db, UserManager<AppUser> userManager) =>
{
    var invite = await db.Invites.FirstOrDefaultAsync(i => i.Token == token);
    if (invite is null)
        return Results.NotFound(new { error = "Invite not found" });

    if (invite.IsUsed)
        return Results.BadRequest(new { error = "This invite has already been used" });

    if (invite.ExpiresAt <= DateTime.UtcNow)
        return Results.BadRequest(new { error = "This invite has expired" });

    var existing = await userManager.FindByEmailAsync(invite.Email);
    if (existing is not null)
        return Results.Conflict(new { error = "An account with this email already exists" });

    var user = new AppUser
    {
        UserName = invite.Email,
        Email = invite.Email,
        DisplayName = req.DisplayName,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(user, req.Password);
    if (!result.Succeeded)
        return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });

    await userManager.AddToRoleAsync(user, invite.Role);
    await CreatePersonalDashboardAsync(db, user.Id, user.DisplayName);

    // Mark invite as used
    invite.IsUsed = true;
    invite.UsedAt = DateTime.UtcNow;
    invite.AcceptedByUserId = user.Id;
    await db.SaveChangesAsync();

    var roles = await userManager.GetRolesAsync(user);
    return Results.Ok(new UserDto(user.Id, user.Email!, user.DisplayName, user.AvatarUrl, roles.ToArray(), user.CreatedAt, user.LastLoginAt));
});

adminApi.MapGet("/stats", async (UserManager<AppUser> userManager, DashboardDbContext db) =>
{
    var users = await userManager.Users.ToListAsync();
    var totalUsers = users.Count;
    var adminCount = 0;
    foreach (var u in users)
    {
        var roles = await userManager.GetRolesAsync(u);
        if (roles.Contains("Admin")) adminCount++;
    }

    var recentUsers = users
        .OrderByDescending(u => u.CreatedAt)
        .Take(5)
        .Select(u => new { u.DisplayName, u.Email, u.CreatedAt, u.LastLoginAt })
        .ToList();

    var dashboards = await db.Dashboards.ToListAsync();
    var totalDashboards = dashboards.Count;
    var sharedDashboards = dashboards.Count(d => d.Visibility == "Shared");

    var widgets = await db.Widgets.Where(w => w.ParentWidgetId == null).ToListAsync();
    var totalWidgets = widgets.Count;
    var widgetsByType = widgets
        .GroupBy(w => w.Type)
        .ToDictionary(g => g.Key, g => g.Count());

    return Results.Ok(new
    {
        totalUsers,
        adminCount,
        totalDashboards,
        sharedDashboards,
        totalWidgets,
        widgetsByType,
        recentUsers
    });
});

// ── API Keys ─────────────────────────────────────────────────────────────────
var keysApi = api.MapGroup("/keys");

keysApi.MapPost("/", async (HttpRequest request, DashboardDbContext db, CreateApiKeyRequest req) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var user = await db.Users.FindAsync(userId);
    if (user == null) return Results.Unauthorized();

    var isAdmin = await db.UserRoles
        .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
        .AnyAsync(x => x.UserId == userId && x.Name == "Admin");

    if (req.IsGlobal && !isAdmin) return Results.StatusCode(403);
    if (req.AllowedDashboardIds != null && !isAdmin) return Results.StatusCode(403);

    var (rawKey, hash, prefix) = ApiKeyHelper.GenerateKey();

    var entity = new ApiKeyEntity
    {
        Name = req.Name,
        KeyHash = hash,
        KeyPrefix = prefix,
        OwnerId = userId,
        IsGlobal = req.IsGlobal,
        AllowedDashboardIds = req.AllowedDashboardIds != null
            ? JsonSerializer.Serialize(req.AllowedDashboardIds)
            : null,
        ExpiresAt = req.ExpiresAt,
    };

    db.ApiKeys.Add(entity);
    await db.SaveChangesAsync();

    return Results.Created($"/api/keys/{entity.Id}", new
    {
        id = entity.Id,
        name = entity.Name,
        key = rawKey,
        prefix = entity.KeyPrefix,
        createdAt = entity.CreatedAt,
        expiresAt = entity.ExpiresAt,
        isGlobal = entity.IsGlobal,
        allowedDashboardIds = req.AllowedDashboardIds,
    });
});

keysApi.MapGet("/", async (HttpRequest request, DashboardDbContext db) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var keys = await db.ApiKeys
        .Where(k => k.OwnerId == userId && !k.IsRevoked)
        .OrderByDescending(k => k.CreatedAt)
        .Select(k => new
        {
            k.Id, k.Name, Prefix = k.KeyPrefix, k.CreatedAt, k.ExpiresAt, k.LastUsedAt, k.IsGlobal,
            allowedDashboardIds = k.AllowedDashboardIds == null ? null
                : JsonSerializer.Deserialize<List<string>>(k.AllowedDashboardIds),
        })
        .ToListAsync();

    return Results.Ok(keys);
});

keysApi.MapDelete("/{id}", async (string id, HttpRequest request, DashboardDbContext db) =>
{
    var userId = request.Headers["X-User-Id"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var key = await db.ApiKeys.FindAsync(id);
    if (key == null) return Results.NotFound();

    var isAdmin = await db.UserRoles
        .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
        .AnyAsync(x => x.UserId == userId && x.Name == "Admin");

    if (key.OwnerId != userId && !isAdmin) return Results.StatusCode(403);

    key.IsRevoked = true;
    await db.SaveChangesAsync();

    return Results.NoContent();
});

// ── CVE Tracking Proxy ────────────────────────────────────────────────────────

api.MapGet("/cve/nvd", async (int? limit, string? keywords, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("nvd");
    try
    {
        // Parse keywords for filtering
        var keywordList = (keywords ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(k => k.ToLowerInvariant())
            .ToArray();

        // Get CVEs from the last 7 days with HIGH or CRITICAL severity
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-7);
        var startStr = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        var endStr = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        var maxResults = Math.Min(limit ?? 20, 50);

        // First try CRITICAL, then HIGH
        var cves = new List<CveResultItem>();

        foreach (var severity in new[] { "CRITICAL", "HIGH" })
        {
            if (cves.Count >= maxResults) break;

            // Fetch more results if filtering to ensure we get enough matches
            var fetchCount = keywordList.Length > 0 ? Math.Max(maxResults * 3, 100) : maxResults;
            var url = $"rest/json/cves/2.0?pubStartDate={startStr}&pubEndDate={endStr}&cvssV3Severity={severity}&resultsPerPage={fetchCount}";
            var response = await client.GetFromJsonAsync<NvdCveResponse>(url);

            if (response?.Vulnerabilities is not null)
            {
                foreach (var vuln in response.Vulnerabilities)
                {
                    if (cves.Count >= maxResults) break;

                    var cve = vuln.Cve;
                    var cvssV3 = cve.Metrics?.CvssMetricV31?.FirstOrDefault()
                              ?? cve.Metrics?.CvssMetricV30?.FirstOrDefault();

                    var baseScore = cvssV3?.CvssData?.BaseScore ?? 0;
                    var baseSeverity = cvssV3?.CvssData?.BaseSeverity ?? severity;

                    // Only include if score >= 7
                    if (baseScore < 7) continue;

                    var description = cve.Descriptions?.FirstOrDefault(d => d.Lang == "en")?.Value ?? "";
                    var fullDescription = description; // Keep full description for keyword matching
                    if (description.Length > 200)
                        description = description[..200] + "...";

                    // Extract affected products from CPE data
                    // CPE format: cpe:2.3:part:vendor:product:version:...
                    var affectedProducts = new HashSet<string>();
                    var rawCpeTerms = new HashSet<string>(); // For keyword matching
                    if (cve.Configurations is not null)
                    {
                        foreach (var config in cve.Configurations)
                        {
                            if (config.Nodes is null) continue;
                            foreach (var node in config.Nodes)
                            {
                                if (node.CpeMatch is null) continue;
                                foreach (var match in node.CpeMatch)
                                {
                                    if (string.IsNullOrEmpty(match.Criteria)) continue;
                                    var parts = match.Criteria.Split(':');
                                    if (parts.Length >= 5)
                                    {
                                        var vendor = parts[3].Replace('_', ' ');
                                        var product = parts[4].Replace('_', ' ');
                                        rawCpeTerms.Add(vendor.ToLowerInvariant());
                                        rawCpeTerms.Add(product.ToLowerInvariant());
                                        if (vendor != "*" && product != "*")
                                        {
                                            // Title case the vendor/product
                                            vendor = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(vendor);
                                            product = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(product);
                                            affectedProducts.Add($"{vendor} {product}");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Apply keyword filter if keywords are provided
                    if (keywordList.Length > 0)
                    {
                        var descLower = fullDescription.ToLowerInvariant();
                        var idLower = cve.Id.ToLowerInvariant();
                        var matchesKeyword = keywordList.Any(kw =>
                            rawCpeTerms.Any(t => t.Contains(kw)) ||
                            descLower.Contains(kw) ||
                            idLower.Contains(kw));

                        if (!matchesKeyword) continue;
                    }

                    cves.Add(new CveResultItem(
                        cve.Id,
                        baseSeverity.ToLower(),
                        baseScore,
                        description,
                        cve.Published?.ToString("yyyy-MM-dd") ?? "",
                        $"https://nvd.nist.gov/vuln/detail/{cve.Id}",
                        affectedProducts.Take(3).ToArray()));
                }
            }
        }

        // Sort by published date descending (latest first)
        cves = cves.OrderByDescending(c => c.PublishedDate).ToList();

        return Results.Ok(new { source = "NVD", items = cves });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to fetch CVEs from NVD: {ex.Message}");
    }
});

api.MapGet("/cve/euvd", async (int? limit, string? keywords, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("euvd");
    try
    {
        // Parse keywords for filtering
        var keywordList = (keywords ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(k => k.ToLowerInvariant())
            .ToArray();

        var maxResults = Math.Min(limit ?? 20, 50);

        // Fetch more results if filtering to ensure we get enough matches
        var fetchCount = keywordList.Length > 0 ? Math.Max(maxResults * 3, 100) : maxResults;

        // EUVD API - fetch recent critical/high vulnerabilities
        var response = await client.GetFromJsonAsync<EuvdResponse>(
            $"api/vulnerabilities?size={fetchCount}&sort=datePublished,desc");

        var cves = new List<CveResultItem>();

        if (response?.Content is not null)
        {
            foreach (var vuln in response.Content)
            {
                if (cves.Count >= maxResults) break;

                var baseScore = vuln.BaseScore ?? vuln.CvssScore ?? 0;
                var severity = baseScore switch
                {
                    >= 9.0 => "critical",
                    >= 7.0 => "high",
                    >= 4.0 => "medium",
                    _ => "low"
                };

                // Only include if score >= 7 (high or critical)
                if (baseScore < 7) continue;

                var description = vuln.Description ?? vuln.Summary ?? "";
                var fullDescription = description; // Keep full for keyword matching
                if (description.Length > 200)
                    description = description[..200] + "...";

                // Extract affected product from EUVD
                var affectedProducts = new List<string>();
                if (!string.IsNullOrEmpty(vuln.VendorProduct))
                    affectedProducts.Add(vuln.VendorProduct);
                else if (!string.IsNullOrEmpty(vuln.Product))
                    affectedProducts.Add(vuln.Product);

                // Apply keyword filter if keywords are provided
                if (keywordList.Length > 0)
                {
                    var descLower = fullDescription.ToLowerInvariant();
                    var cveId = (vuln.Id ?? vuln.CveId ?? "").ToLowerInvariant();
                    var productLower = (vuln.VendorProduct ?? vuln.Product ?? "").ToLowerInvariant();

                    var matchesKeyword = keywordList.Any(kw =>
                        productLower.Contains(kw) ||
                        descLower.Contains(kw) ||
                        cveId.Contains(kw));

                    if (!matchesKeyword) continue;
                }

                var id = vuln.Id ?? vuln.CveId ?? "Unknown";
                cves.Add(new CveResultItem(
                    id,
                    severity,
                    baseScore,
                    description,
                    vuln.DatePublished?.ToString("yyyy-MM-dd") ?? "",
                    $"https://euvd.enisa.europa.eu/vulnerability/{id}",
                    affectedProducts.ToArray()));
            }
        }

        // Sort by published date descending (latest first)
        cves = cves.OrderByDescending(c => c.PublishedDate).Take(maxResults).ToList();

        return Results.Ok(new { source = "EUVD", items = cves });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to fetch CVEs from EUVD: {ex.Message}");
    }
});

app.Run();

public record UpdateWidgetContentRequest(string? Content, string? Css);

public record CreateApiKeyRequest(
    string Name,
    DateTime? ExpiresAt = null,
    bool IsGlobal = false,
    List<string>? AllowedDashboardIds = null);

record GeoResponse([property: System.Text.Json.Serialization.JsonPropertyName("results")] GeoResult[]? Results);
record GeoResult(
    [property: System.Text.Json.Serialization.JsonPropertyName("latitude")] double Latitude,
    [property: System.Text.Json.Serialization.JsonPropertyName("longitude")] double Longitude,
    [property: System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
    [property: System.Text.Json.Serialization.JsonPropertyName("country")] string Country);
record WeatherResponse([property: System.Text.Json.Serialization.JsonPropertyName("current")] WeatherCurrent? Current);
record WeatherCurrent(
    [property: System.Text.Json.Serialization.JsonPropertyName("temperature_2m")] double Temperature2m,
    [property: System.Text.Json.Serialization.JsonPropertyName("relative_humidity_2m")] int RelativeHumidity2m,
    [property: System.Text.Json.Serialization.JsonPropertyName("wind_speed_10m")] double WindSpeed10m,
    [property: System.Text.Json.Serialization.JsonPropertyName("cloud_cover")] int CloudCover,
    [property: System.Text.Json.Serialization.JsonPropertyName("weather_code")] int WeatherCode);

// CVE API response types
record CveResultItem(string Id, string Severity, double Score, string Description, string PublishedDate, string Url, string[] AffectedProducts);

// NVD API response types
record NvdCveResponse(
    [property: System.Text.Json.Serialization.JsonPropertyName("vulnerabilities")] NvdVulnerability[]? Vulnerabilities);
record NvdVulnerability(
    [property: System.Text.Json.Serialization.JsonPropertyName("cve")] NvdCve Cve);
record NvdCve(
    [property: System.Text.Json.Serialization.JsonPropertyName("id")] string Id,
    [property: System.Text.Json.Serialization.JsonPropertyName("published")] DateTime? Published,
    [property: System.Text.Json.Serialization.JsonPropertyName("descriptions")] NvdDescription[]? Descriptions,
    [property: System.Text.Json.Serialization.JsonPropertyName("metrics")] NvdMetrics? Metrics,
    [property: System.Text.Json.Serialization.JsonPropertyName("configurations")] NvdConfiguration[]? Configurations);
record NvdDescription(
    [property: System.Text.Json.Serialization.JsonPropertyName("lang")] string Lang,
    [property: System.Text.Json.Serialization.JsonPropertyName("value")] string Value);
record NvdMetrics(
    [property: System.Text.Json.Serialization.JsonPropertyName("cvssMetricV31")] NvdCvssMetric[]? CvssMetricV31,
    [property: System.Text.Json.Serialization.JsonPropertyName("cvssMetricV30")] NvdCvssMetric[]? CvssMetricV30);
record NvdCvssMetric(
    [property: System.Text.Json.Serialization.JsonPropertyName("cvssData")] NvdCvssData? CvssData);
record NvdCvssData(
    [property: System.Text.Json.Serialization.JsonPropertyName("baseScore")] double BaseScore,
    [property: System.Text.Json.Serialization.JsonPropertyName("baseSeverity")] string BaseSeverity);

// NVD CPE configuration types
record NvdConfiguration(
    [property: System.Text.Json.Serialization.JsonPropertyName("nodes")] NvdConfigNode[]? Nodes);
record NvdConfigNode(
    [property: System.Text.Json.Serialization.JsonPropertyName("cpeMatch")] NvdCpeMatch[]? CpeMatch);
record NvdCpeMatch(
    [property: System.Text.Json.Serialization.JsonPropertyName("criteria")] string? Criteria);

// EUVD API response types
record EuvdResponse(
    [property: System.Text.Json.Serialization.JsonPropertyName("content")] EuvdVulnerability[]? Content);
record EuvdVulnerability(
    [property: System.Text.Json.Serialization.JsonPropertyName("id")] string? Id,
    [property: System.Text.Json.Serialization.JsonPropertyName("cveId")] string? CveId,
    [property: System.Text.Json.Serialization.JsonPropertyName("description")] string? Description,
    [property: System.Text.Json.Serialization.JsonPropertyName("summary")] string? Summary,
    [property: System.Text.Json.Serialization.JsonPropertyName("baseScore")] double? BaseScore,
    [property: System.Text.Json.Serialization.JsonPropertyName("cvssScore")] double? CvssScore,
    [property: System.Text.Json.Serialization.JsonPropertyName("datePublished")] DateTime? DatePublished,
    [property: System.Text.Json.Serialization.JsonPropertyName("vendorProduct")] string? VendorProduct,
    [property: System.Text.Json.Serialization.JsonPropertyName("product")] string? Product);

