using Microsoft.EntityFrameworkCore;
using PortModelApi.Models;

namespace PortModelApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PortModelMapping> PortModelMappings { get; set; } = default!;
    public DbSet<PortModelMappingAudit> PortModelMappingAudits { get; set; } = default!;
    public DbSet<Portfolio> Portfolios { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PortModelMapping>()
            .HasKey(p => new { p.AccnoSleeve, p.EffectiveDate });

        // Soft delete filter
        modelBuilder.Entity<PortModelMapping>()
            .HasQueryFilter(p => !p.IsDeleted);
    }
}