using IPrazo.Crowler.Models;
using Microsoft.EntityFrameworkCore;

namespace IPrazo.Crowler.Data;

public class AppDbContext : DbContext
{
    public DbSet<CrawlExecution> Executions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=crawler.db");
}