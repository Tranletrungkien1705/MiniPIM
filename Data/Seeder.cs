using Microsoft.EntityFrameworkCore;
using MiniPIM.Models;

namespace MiniPIM.Data;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await MigratePostgresAsync(db);

        if (!await db.Orgs.AnyAsync(o => o.Id == TenantContext.DefaultOrgId))
        {
            db.Orgs.Add(new Org { Id = TenantContext.DefaultOrgId, Name = "Demo PIM", ApiKey = TenantContext.DefaultApiKey });
            await db.SaveChangesAsync();
        }
        if (!await db.Groups.AnyAsync())
        {
            db.Groups.AddRange(
                new ProductGroup { Code = "AO", Name = "Áo" },
                new ProductGroup { Code = "QUAN", Name = "Quần" },
                new ProductGroup { Code = "PK", Name = "Phụ kiện" });
            await db.SaveChangesAsync();
        }
        if (!await db.AttributeDefs.AnyAsync())
        {
            db.AttributeDefs.AddRange(
                new AttributeDef { Code = "CHATLIEU", Name = "Chất liệu" },
                new AttributeDef { Code = "MAU", Name = "Màu sắc" },
                new AttributeDef { Code = "SIZE", Name = "Kích cỡ" },
                new AttributeDef { Code = "XUATXU", Name = "Xuất xứ" });
            await db.SaveChangesAsync();
        }
        if (!await db.Products.AnyAsync())
        {
            var groups = await db.Groups.ToListAsync();
            int GId(string c) => groups.First(g => g.Code == c).Id;
            var p1 = new Product { Code = "AO-001", Name = "Áo sơ mi trắng basic", GroupId = GId("AO"), Uom = "cái", Barcode = "8930001", CostPrice = 120000, SalePrice = 250000,
                Level = ProductLevel.Base, ValConvert = 1, QtyMinSt = 20, QtyMaxSt = 500, VatRateCode = "VAT10", Origin = "Việt Nam", QuyCach = "1 cái/túi",
                Attributes = [ new() { Name = "Chất liệu", Value = "Cotton 100%" }, new() { Name = "Màu", Value = "Trắng" } ] };
            var p2 = new Product { Code = "QUAN-001", Name = "Quần jeans slim", GroupId = GId("QUAN"), Uom = "cái", CostPrice = 200000, SalePrice = 450000,
                Level = ProductLevel.Base, ValConvert = 1, QtyMinSt = 10, QtyMaxSt = 300, VatRateCode = "VAT10", Origin = "Việt Nam",
                Attributes = [ new() { Name = "Chất liệu", Value = "Denim" } ],
                Bom = [ new() { ComponentCode = "VAI-DENIM", ComponentName = "Vải denim", Quantity = 1.2m, Uom = "m" }, new() { ComponentCode = "KHOA", ComponentName = "Khóa kéo", Quantity = 1, Uom = "cái" } ] };
            var p3 = new Product { Code = "PK-001", Name = "Thắt lưng da", GroupId = GId("PK"), Uom = "cái", CostPrice = 80000, SalePrice = 180000,
                Level = ProductLevel.L2, ValConvert = 1, VatRateCode = "VAT10" };
            db.Products.AddRange(p1, p2, p3);
            await db.SaveChangesAsync();
        }
        if (!await db.Specs.AnyAsync())
        {
            var s1 = new Spec { Code = "AO-001-DENIM-M", Name = "Áo sơ mi trắng basic - Denim M", ModelCode = "AO-001", SpecType1 = "SIZE", SpecType2 = "MAU", Color = "Trắng",
                FlagHasSerial = false, FlagHasLot = false, DefaultUnitCode = "cái", StandardUnitCode = "cái", Remark = "Quy cách bán lẻ",
                Prices = [ new() { SpecCode = "AO-001-DENIM-M", UnitCode = "cái", BuyPrice = 120000, SellPrice = 250000, CurrencyCode = "VND", VatRateCode = "VAT10" } ] };
            var s2 = new Spec { Code = "QUAN-001-DENIM-32", Name = "Quần jeans slim - Denim 32", ModelCode = "QUAN-001", SpecType1 = "SIZE", Color = "Xanh",
                FlagHasSerial = false, FlagHasLot = true, DefaultUnitCode = "cái", StandardUnitCode = "cái",
                Prices = [ new() { SpecCode = "QUAN-001-DENIM-32", UnitCode = "cái", BuyPrice = 200000, SellPrice = 450000, CurrencyCode = "VND", VatRateCode = "VAT10", DiscountVnd = 20000 } ] };
            db.Specs.AddRange(s1, s2);
            await db.SaveChangesAsync();
        }
    }

    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Groups", "Products", "Attributes", "BomLines", "AttributeDefs", "Specs", "SpecPrices" };
        var sql = new List<string>
        {
            "CREATE TABLE IF NOT EXISTS minipim.\"Orgs\" (\"Id\" uuid PRIMARY KEY, \"Name\" text NOT NULL DEFAULT '', \"ApiKey\" text NOT NULL DEFAULT '', \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Orgs_ApiKey\" ON minipim.\"Orgs\" (\"ApiKey\")",
        };
        foreach (var t in tables) sql.Add($"ALTER TABLE minipim.\"{t}\" ADD COLUMN IF NOT EXISTS \"OrgId\" uuid NOT NULL DEFAULT '{def}'");
        foreach (var s in sql) try { await db.Database.ExecuteSqlRawAsync(s); } catch { }
    }
}
