using dashy3.ApiService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dashy3.ApiService.Data;

public class DashboardDbContext(DbContextOptions<DashboardDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<DashboardEntity> Dashboards => Set<DashboardEntity>();
    public DbSet<WidgetEntity> Widgets => Set<WidgetEntity>();
    public DbSet<OidcConfigEntity> OidcConfig => Set<OidcConfigEntity>();
    public DbSet<DashboardCollaboratorEntity> Collaborators => Set<DashboardCollaboratorEntity>();
    public DbSet<ApiKeyEntity> ApiKeys => Set<ApiKeyEntity>();
    public DbSet<InviteEntity> Invites => Set<InviteEntity>();
    public DbSet<EmailConfigEntity> EmailConfig => Set<EmailConfigEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // IMPORTANT: call base for Identity tables

        modelBuilder.Entity<DashboardEntity>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).IsRequired();
            e.HasMany(d => d.Widgets).WithOne(w => w.Dashboard).HasForeignKey(w => w.DashboardId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WidgetEntity>(e =>
        {
            e.HasKey(w => w.Id);
            e.Property(w => w.Type).IsRequired();
            e.Property(w => w.ConfigJson).HasColumnType("text").HasDefaultValue("{}");

            e.HasOne(w => w.ParentWidget)
                .WithMany(w => w.ChildWidgets)
                .HasForeignKey(w => w.ParentWidgetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OidcConfigEntity>(e =>
        {
            e.HasKey(o => o.Id);
            // Seed a default singleton record — all properties must be static to avoid PendingModelChangesWarning
            e.HasData(new OidcConfigEntity
            {
                Id = 1,
                Authority = "",
                ClientId = "",
                ClientSecret = "",
                Scopes = "openid profile email",
                CallbackPath = "/signin-oidc",
                IsEnabled = false,
                AllowAutoRegistration = true,
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        });

        modelBuilder.Entity<DashboardCollaboratorEntity>(e =>
        {
            e.HasKey(c => new { c.DashboardId, c.UserId });
            e.HasOne(c => c.Dashboard)
                .WithMany()
                .HasForeignKey(c => c.DashboardId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiKeyEntity>(e =>
        {
            e.HasKey(k => k.Id);
            e.HasIndex(k => k.KeyHash).IsUnique();
            e.HasIndex(k => k.OwnerId);
            e.HasOne(k => k.Owner)
                .WithMany()
                .HasForeignKey(k => k.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InviteEntity>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasIndex(i => i.Token).IsUnique();
            e.HasIndex(i => i.Email);
        });

        modelBuilder.Entity<EmailConfigEntity>(e =>
        {
            e.HasKey(ec => ec.Id);
            // Seed a default singleton record
            e.HasData(new EmailConfigEntity
            {
                Id = 1,
                Provider = "None",
                IsEnabled = false,
                SmtpHost = "",
                SmtpPort = 587,
                SmtpUseSsl = true,
                SmtpUsername = "",
                SmtpPassword = "",
                SmtpFromEmail = "",
                SmtpFromName = "Dashy",
                GraphTenantId = "",
                GraphClientId = "",
                GraphClientSecret = "",
                GraphFromEmail = "",
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        });
    }
}
