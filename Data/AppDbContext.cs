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
    public DbSet<AttributeDef> AttributeDefs => Set<AttributeDef>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<VatRate> VatRates => Set<VatRate>();
    public DbSet<Spec> Specs => Set<Spec>();
    public DbSet<SpecPrice> SpecPrices => Set<SpecPrice>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Model> Models => Set<Model>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<CurrencyEx> CurrencyExes => Set<CurrencyEx>();
    public DbSet<SpecUnit> SpecUnits => Set<SpecUnit>();
    public DbSet<SpecCustomField> SpecCustomFields => Set<SpecCustomField>();
    public DbSet<SpecType> SpecTypes => Set<SpecType>();
    public DbSet<SsccType> SsccTypes => Set<SsccType>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductFile> ProductFiles => Set<ProductFile>();
    public DbSet<ProductCustomField> ProductCustomFields => Set<ProductCustomField>();
    public DbSet<SpecImage> SpecImages => Set<SpecImage>();
    public DbSet<SpecFile> SpecFiles => Set<SpecFile>();

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
            e.HasIndex(x => new { x.OrgId, x.CodeUser }).IsUnique();
            e.Property(x => x.CostPrice).HasPrecision(18, 2);
            e.Property(x => x.SalePrice).HasPrecision(18, 2);
            e.Property(x => x.ValConvert).HasPrecision(18, 3);
            e.Property(x => x.QtyMinSt).HasPrecision(18, 3);
            e.Property(x => x.QtyMaxSt).HasPrecision(18, 3);
            e.HasOne(x => x.Group).WithMany().HasForeignKey(x => x.GroupId);
            e.HasIndex(x => new { x.OrgId, x.ProductCodeRoot });
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<AttributeDef>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<AttributeDef>().Property(x => x.NetworkId).HasMaxLength(50);
        b.Entity<Unit>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.CodeUser }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.Name }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<VatRate>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.Property(x => x.Rate).HasPrecision(9, 2);
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
            e.HasOne(x => x.Product).WithMany(x => x.Bom).HasForeignKey(x => x.ProductId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Spec>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SpecPrice>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.SpecCode, x.UnitCode }).IsUnique();
            e.Property(x => x.BuyPrice).HasPrecision(18, 2);
            e.Property(x => x.SellPrice).HasPrecision(18, 2);
            e.Property(x => x.DiscountVnd).HasPrecision(18, 2);
            e.HasOne(x => x.Spec).WithMany(x => x.Prices).HasForeignKey(x => x.SpecId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Brand>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Model>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ProductType>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CurrencyEx>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.Property(x => x.BuyRate).HasPrecision(18, 6);
            e.Property(x => x.SellRate).HasPrecision(18, 6);
            e.Property(x => x.InterEx).HasPrecision(18, 6);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SpecUnit>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.SpecCode, x.UnitCode }).IsUnique();
            e.Property(x => x.Qty).HasPrecision(18, 3);
            e.Property(x => x.Length).HasPrecision(18, 3);
            e.Property(x => x.Width).HasPrecision(18, 3);
            e.Property(x => x.Height).HasPrecision(18, 3);
            e.Property(x => x.Volume).HasPrecision(18, 3);
            e.Property(x => x.Weight).HasPrecision(18, 3);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SpecCustomField>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SpecType>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Kind, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SsccType>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ProductImage>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ProductCode, x.Idx });
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ProductFile>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ProductCode, x.Idx });
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ProductCustomField>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SpecImage>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.SpecCode });
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SpecFile>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.SpecCode });
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
