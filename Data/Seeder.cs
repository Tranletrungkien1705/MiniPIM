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
                new ProductGroup { Code = "AO", Name = "Áo", BUCode = "AO", BUPattern = "AO%", Level = 0, FlagFG = true },
                new ProductGroup { Code = "AO-SOMI", Name = "Áo sơ mi", ParentCode = "AO", BUCode = "AO.AO-SOMI", BUPattern = "AO.AO-SOMI%", Level = 1, FlagFG = true },
                new ProductGroup { Code = "QUAN", Name = "Quần", BUCode = "QUAN", BUPattern = "QUAN%", Level = 0, FlagFG = true },
                new ProductGroup { Code = "PK", Name = "Phụ kiện", BUCode = "PK", BUPattern = "PK%", Level = 0, FlagFG = false });
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
        if (!await db.Units.AnyAsync())
        {
            db.Units.AddRange(
                new Unit { Code = "UOM0001", CodeUser = "CAI", Name = "Cái", Remark = "Đơn vị đếm cơ bản" },
                new Unit { Code = "UOM0002", CodeUser = "CHIEC", Name = "Chiếc" },
                new Unit { Code = "UOM0003", CodeUser = "BO", Name = "Bộ" },
                new Unit { Code = "UOM0004", CodeUser = "KG", Name = "Kilôgam" },
                new Unit { Code = "UOM0005", CodeUser = "M", Name = "Mét", Remark = "Dùng cho vải, dây" },
                new Unit { Code = "UOM0006", CodeUser = "THUNG", Name = "Thùng" });
            await db.SaveChangesAsync();
        }
        if (!await db.VatRates.AnyAsync())
        {
            db.VatRates.AddRange(
                new VatRate { Code = "VAT0", Rate = 0, Name = "Không chịu thuế GTGT" },
                new VatRate { Code = "VAT5", Rate = 5, Name = "Thuế GTGT 5%" },
                new VatRate { Code = "VAT8", Rate = 8, Name = "Thuế GTGT 8%" },
                new VatRate { Code = "VAT10", Rate = 10, Name = "Thuế GTGT 10%" });
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
        if (!await db.Brands.AnyAsync())
        {
            db.Brands.AddRange(
                new Brand { Code = "BRAND-A", Name = "Thương hiệu A", NetworkBrandCode = "BRAND-A" },
                new Brand { Code = "BRAND-B", Name = "Thương hiệu B", NetworkBrandCode = "BRAND-B" });
            await db.SaveChangesAsync();
        }
        if (!await db.Models.AnyAsync())
        {
            db.Models.AddRange(
                new Model { Code = "AO-001", Name = "Áo sơ mi basic", BrandCode = "BRAND-A", OrgModelCode = "AO-001", NetworkModelCode = "AO-001", Remark = "Dòng áo sơ mi cơ bản" },
                new Model { Code = "QUAN-001", Name = "Quần jeans slim", BrandCode = "BRAND-A", OrgModelCode = "QUAN-001", NetworkModelCode = "QUAN-001" },
                new Model { Code = "PK-001", Name = "Thắt lưng da", BrandCode = "BRAND-B", OrgModelCode = "PK-001" });
            await db.SaveChangesAsync();
        }
        if (!await db.ProductTypes.AnyAsync())
        {
            db.ProductTypes.AddRange(
                new ProductType { Code = "NORMAL", Name = "Hàng thường", Remark = "Hàng hóa bán lẻ thông thường" },
                new ProductType { Code = "COMBO", Name = "Hàng combo", Remark = "Bán theo bộ thành phần (BOM)" },
                new ProductType { Code = "SERVICE", Name = "Dịch vụ", Remark = "Hàng hóa không quản lý tồn kho" });
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
        var tables = new[] { "Groups", "Products", "Attributes", "BomLines", "AttributeDefs", "Specs", "SpecPrices", "Units", "VatRates", "Brands", "Models", "ProductTypes" };
        var sql = new List<string>
        {
            "CREATE TABLE IF NOT EXISTS minipim.\"Orgs\" (\"Id\" uuid PRIMARY KEY, \"Name\" text NOT NULL DEFAULT '', \"ApiKey\" text NOT NULL DEFAULT '', \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Orgs_ApiKey\" ON minipim.\"Orgs\" (\"ApiKey\")",
        };
        foreach (var t in tables) sql.Add($"ALTER TABLE minipim.\"{t}\" ADD COLUMN IF NOT EXISTS \"OrgId\" uuid NOT NULL DEFAULT '{def}'");
        // Nhóm hàng phân cấp (Mst_ProductGroup): đường dẫn BU + cờ.
        sql.Add("ALTER TABLE minipim.\"Groups\" ADD COLUMN IF NOT EXISTS \"ParentCode\" text NULL");
        sql.Add("ALTER TABLE minipim.\"Groups\" ADD COLUMN IF NOT EXISTS \"BUCode\" text NOT NULL DEFAULT ''");
        sql.Add("ALTER TABLE minipim.\"Groups\" ADD COLUMN IF NOT EXISTS \"BUPattern\" text NOT NULL DEFAULT ''");
        sql.Add("ALTER TABLE minipim.\"Groups\" ADD COLUMN IF NOT EXISTS \"Level\" integer NOT NULL DEFAULT 0");
        sql.Add("ALTER TABLE minipim.\"Groups\" ADD COLUMN IF NOT EXISTS \"FlagFG\" boolean NOT NULL DEFAULT false");
        sql.Add("ALTER TABLE minipim.\"Groups\" ADD COLUMN IF NOT EXISTS \"Active\" boolean NOT NULL DEFAULT true");
        sql.Add("ALTER TABLE minipim.\"Groups\" ADD COLUMN IF NOT EXISTS \"UpdatedAt\" timestamp NOT NULL DEFAULT now()");
        // Loại hàng hóa (Mst_ProductType): cột tham chiếu trên Hàng hóa.
        sql.Add("ALTER TABLE minipim.\"Products\" ADD COLUMN IF NOT EXISTS \"ProductTypeCode\" text NULL");
        foreach (var s in sql) try { await db.Database.ExecuteSqlRawAsync(s); } catch { }
    }
}
