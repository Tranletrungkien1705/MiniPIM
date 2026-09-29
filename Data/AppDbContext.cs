using Microsoft.EntityFrameworkCore;
using MiniPIM.Models;

namespace MiniPIM.Data;

public class AppDbContext : DbContext
{
    private readonly Guid _orgId;
    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenant) : base(options) => _orgId = tenant.OrgId;

    public DbSet<Org> Orgs => Set<Org>();
    public DbSet<ProductGroup> Groups => Set<ProductGroup>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductAttribute> Attributes => Set<ProductAttribute>();
    public DbSet<BomLine> BomLines => Set<BomLine>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        if (Database.IsNpgsql()) b.HasDefaultSchema("minipim");
        b.Entity<Org>().HasIndex(x => x.ApiKey).IsUnique();
        b.Entity<ProductGroup>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Product>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.Property(x => x.CostPrice).HasPrecision(18, 2);
            e.Property(x => x.SalePrice).HasPrecision(18, 2);
            e.Property(x => x.QtyEffSt).HasPrecision(18, 3);     // QtyEffSt — tồn tối ưu, Mst_Product.cs:40
            e.HasOne(x => x.Group).WithMany().HasForeignKey(x => x.GroupId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ProductAttribute>(e =>
        {
            e.HasOne(x => x.Product).WithMany(x => x.Attributes).HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<BomLine>(e =>
        {
            e.Property(x => x.Quantity).HasPrecision(18, 3);
            e.Property(x => x.ComponentCostPrice).HasPrecision(18, 2);   // mp_UPBuy
            e.Property(x => x.ComponentSalePrice).HasPrecision(18, 2);   // mp_UPSell
            e.Ignore(x => x.CostAmount);    // công thức tính, không lưu DB (ProductIngredient.cshtml:95)
            e.Ignore(x => x.SaleAmount);    // công thức tính, không lưu DB (ProductIngredient.cshtml:96)
            e.HasOne(x => x.Product).WithMany(x => x.Bom).HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
    }

    public override int SaveChanges() { StampOrg(); return base.SaveChanges(); }
    public override Task<int> SaveChangesAsync(CancellationToken ct = default) { StampOrg(); return base.SaveChangesAsync(ct); }
    private void StampOrg()
    {
        foreach (var e in ChangeTracker.Entries<IOrgOwned>())
            if (e.State == EntityState.Added && e.Entity.OrgId == Guid.Empty) e.Entity.OrgId = _orgId;
    }
}
