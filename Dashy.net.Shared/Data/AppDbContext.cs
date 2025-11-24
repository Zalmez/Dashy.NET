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
    public DbSet<AppSettings> AppSettings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuthenticationProvider>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProviderType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

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

        modelBuilder.Entity<AuthenticationProviderSettings>()
            .HasIndex(e => new { e.AuthenticationProviderId, e.Key })
            .IsUnique();

        // Self-referencing hierarchy for DashboardItem (optional parent container widget)
        modelBuilder.Entity<DashboardItem>()
            .HasOne(i => i.ParentItem)
            .WithMany(p => p.Children)
            .HasForeignKey(i => i.ParentItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override int SaveChanges()
    {
        TouchLastModified();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TouchLastModified();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void TouchLastModified()
    {
        var utcNow = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                switch (entry.Entity)
                {
                    case Dashboard d:
                        d.LastModifiedUtc = utcNow;
                        break;
                    case DashboardSection s:
                        s.LastModifiedUtc = utcNow;
                        // bump parent dashboard too
                        if (entry.State == EntityState.Added || entry.References.Any(r => r.TargetEntry?.Entity is Dashboard))
                        {
                            Entry(s.Dashboard).Property(nameof(Dashboard.LastModifiedUtc)).CurrentValue = utcNow;
                        }
                        break;
                    case DashboardItem i:
                        i.LastModifiedUtc = utcNow;
                        // bump parent section/dashboard
                        if (i.Section is not null)
                        {
                            Entry(i.Section).Property(nameof(DashboardSection.LastModifiedUtc)).CurrentValue = utcNow;
                            if (i.Section.Dashboard is not null)
                            {
                                Entry(i.Section.Dashboard).Property(nameof(Dashboard.LastModifiedUtc)).CurrentValue = utcNow;
                            }
                        }
                        break;
                }
            }
        }
    }
}