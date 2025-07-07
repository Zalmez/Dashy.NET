using Microsoft.EntityFrameworkCore;
using Dashy.Net.Shared.Models;

namespace Dashy.Net.Shared.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Dashboard> Dashboards { get; set; }
    public DbSet<DashboardSection> Sections { get; set; } = null!;
    public DbSet<HeaderButton> HeaderButtons { get; set; }
    public DbSet<DashboardItem> Items { get; set; } = null!;
    public DbSet<AuthenticationSettings> AuthenticationSettings { get; set; } = null!;
    public DbSet<AuthenticationProvider> AuthenticationProviders { get; set; } = null!;
    public DbSet<AuthenticationProviderSettings> AuthenticationProviderSettings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure AuthenticationProvider
        modelBuilder.Entity<AuthenticationProvider>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProviderType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Configure AuthenticationProviderSettings
        modelBuilder.Entity<AuthenticationProviderSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(2000);
            
            entity.HasOne(e => e.AuthenticationProvider)
                .WithMany(e => e.Settings)
                .HasForeignKey(e => e.AuthenticationProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure unique index on provider settings key per provider
        modelBuilder.Entity<AuthenticationProviderSettings>()
            .HasIndex(e => new { e.AuthenticationProviderId, e.Key })
            .IsUnique();
    }
}