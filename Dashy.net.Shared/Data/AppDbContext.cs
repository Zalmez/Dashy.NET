using Microsoft.EntityFrameworkCore;
using Dashy.Net.Shared.Models;

namespace Dashy.Net.Shared.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DashboardSection> Sections { get; set; } = null!;
    public DbSet<DashboardItem> Items { get; set; } = null!;
    
}