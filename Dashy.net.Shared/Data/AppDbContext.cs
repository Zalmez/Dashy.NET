using Microsoft.EntityFrameworkCore;
using Dashy.Net.Shared.Models;

namespace Dashy.Net.Shared.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Dashboard> Dashboards { get; set; }
    public DbSet<DashboardSection> Sections { get; set; } = null!;
    public DbSet<HeaderButton> HeaderButtons { get; set; }
    public DbSet<DashboardItem> Items { get; set; } = null!;
    
}