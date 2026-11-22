using DigitalProductPass.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalProductPass.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<SupplyChainStep> SupplyChainSteps => Set<SupplyChainStep>();
    public DbSet<ProductCertificate> ProductCertificates => Set<ProductCertificate>();
    public DbSet<ProductItem> ProductItems => Set<ProductItem>();
    public DbSet<ScanEvent> ScanEvents => Set<ScanEvent>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Product>().HasIndex(p => new { p.BrandId, p.Sku }).IsUnique();
        b.Entity<Product>().Property(p => p.CarbonFootprintKg).HasPrecision(10, 2);

        b.Entity<Material>().Property(m => m.Percentage).HasPrecision(5, 2);

        b.Entity<ProductItem>().HasIndex(i => i.PublicId).IsUnique();
        b.Entity<ProductItem>().HasIndex(i => i.SerialCode).IsUnique();

        b.Entity<ScanEvent>().HasIndex(s => s.ScannedAt);
    }
}
