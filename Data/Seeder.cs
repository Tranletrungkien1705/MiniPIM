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
                new AttributeDef { Code = "CHATLIEU", Name = "Chất liệu", NetworkId = "CHATLIEU" },
                new AttributeDef { Code = "MAU", Name = "Màu sắc", NetworkId = "MAU" },
                new AttributeDef { Code = "SIZE", Name = "Kích cỡ", NetworkId = "SIZE" },
                new AttributeDef { Code = "XUATXU", Name = "Xuất xứ", NetworkId = "XUATXU" });
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
            var p1 = new Product { Code = "AO-001", CodeUser = "AO-001", Name = "Áo sơ mi trắng basic", GroupId = GId("AO"), Uom = "cái", Barcode = "8930001", CostPrice = 120000, SalePrice = 250000,
                Level = ProductLevel.Base, ValConvert = 1, QtyMinSt = 20, QtyMaxSt = 500, VatRateCode = "VAT10", Origin = "Việt Nam", QuyCach = "1 cái/túi",
                ProductCodeRoot = "AO-001", ProductCodeBase = "AO-001",
                Attributes = [ new() { Name = "Chất liệu", Value = "Cotton 100%" }, new() { Name = "Màu", Value = "Trắng" } ] };
            var p2 = new Product { Code = "QUAN-001", CodeUser = "QUAN-001", Name = "Quần jeans slim", GroupId = GId("QUAN"), Uom = "cái", CostPrice = 200000, SalePrice = 450000,
                Level = ProductLevel.Base, ValConvert = 1, QtyMinSt = 10, QtyMaxSt = 300, VatRateCode = "VAT10", Origin = "Việt Nam",
                ProductCodeRoot = "QUAN-001", ProductCodeBase = "QUAN-001",
                Attributes = [ new() { Name = "Chất liệu", Value = "Denim" } ],
                Bom = [ new() { ComponentCode = "VAI-DENIM", ComponentName = "Vải denim", Quantity = 1.2m, Uom = "m" }, new() { ComponentCode = "KHOA", ComponentName = "Khóa kéo", Quantity = 1, Uom = "cái" } ] };
            var p3 = new Product { Code = "PK-001", CodeUser = "PK-001", Name = "Thắt lưng da", GroupId = GId("PK"), Uom = "cái", CostPrice = 80000, SalePrice = 180000,
                Level = ProductLevel.L2, ValConvert = 1, VatRateCode = "VAT10", DTimeUsed = new DateTime(2024, 1, 1),
                ProductCodeRoot = "PK-001", ProductCodeBase = "PK-001" };
            db.Products.AddRange(p1, p2, p3);
            await db.SaveChangesAsync();
        }
        // Phân cấp hàng hóa (Mst_Product: Root / Base / L2) — hàng gốc + hàng cơ sở + hàng cấp 2 mẫu.
        if (!await db.Products.AnyAsync(p => p.Level == ProductLevel.Root))
        {
            var groups = await db.Groups.ToListAsync();
            int GId(string c) => groups.First(g => g.Code == c).Id;
            var root = new Product { Code = "AO-ROOT", Name = "Áo (gốc)", GroupId = GId("AO"), Uom = "cái", SalePrice = 0,
                Level = ProductLevel.Root, ValConvert = 1, ProductCodeRoot = "AO-ROOT", ProductCodeBase = "AO-ROOT" };
            var basePrd = new Product { Code = "AO-ROOT-SOMI", Name = "Áo sơ mi (cơ sở)", GroupId = GId("AO"), Uom = "cái", SalePrice = 0,
                Level = ProductLevel.Base, ValConvert = 1, ProductCodeRoot = "AO-ROOT", ProductCodeBase = "AO-ROOT-SOMI" };
            var l2Prd = new Product { Code = "AO-ROOT-SOMI-TRANG", Name = "Áo sơ mi trắng (cấp 2)", GroupId = GId("AO"), Uom = "cái", SalePrice = 250000,
                Level = ProductLevel.L2, ValConvert = 1, ProductCodeRoot = "AO-ROOT", ProductCodeBase = "AO-ROOT-SOMI" };
            db.Products.AddRange(root, basePrd, l2Prd);
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
        if (!await db.CurrencyExes.AnyAsync())
        {
            db.CurrencyExes.AddRange(
                new CurrencyEx { Code = "VND", Name = "Việt Nam Đồng", BaseCurrencyCode = null, BuyRate = 1, SellRate = 1, InterEx = 1, Remark = "Đồng tiền gốc" },
                new CurrencyEx { Code = "USD", Name = "Đô la Mỹ", BaseCurrencyCode = "VND", BuyRate = 24500, SellRate = 24700, InterEx = 24600 },
                new CurrencyEx { Code = "EUR", Name = "Euro", BaseCurrencyCode = "VND", BuyRate = 26500, SellRate = 26800, InterEx = 26650 });
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
        if (!await db.SpecUnits.AnyAsync())
        {
            db.SpecUnits.AddRange(
                new SpecUnit { SpecCode = "AO-001-DENIM-M", UnitCode = "CAI", StandardUnitCode = "CAI", Description = "Bán lẻ theo cái", Qty = 1, Weight = 0.25m },
                new SpecUnit { SpecCode = "AO-001-DENIM-M", UnitCode = "THUNG", StandardUnitCode = "CAI", Description = "Thùng 20 cái", Qty = 20, Length = 60, Width = 40, Height = 30, Volume = 72000, Weight = 5.2m },
                new SpecUnit { SpecCode = "QUAN-001-DENIM-32", UnitCode = "CAI", StandardUnitCode = "CAI", Description = "Bán lẻ theo cái", Qty = 1, Weight = 0.6m });
            await db.SaveChangesAsync();
        }
        if (!await db.SpecCustomFields.AnyAsync())
        {
            db.SpecCustomFields.AddRange(
                new SpecCustomField { Code = "SCF001", Name = "Chất liệu vải", DBPhysicalType = "nvarchar(400)", Remark = "Thành phần chất liệu của quy cách" },
                new SpecCustomField { Code = "SCF002", Name = "Xuất xứ", DBPhysicalType = "nvarchar(400)" },
                new SpecCustomField { Code = "SCF003", Name = "Trọng lượng (g)", DBPhysicalType = "decimal(18,3)", Remark = "Khối lượng tịnh" });
            await db.SaveChangesAsync();
        }
        if (!await db.SpecTypes.AnyAsync())
        {
            db.SpecTypes.AddRange(
                new SpecType { Kind = 1, Code = "SIZE", Name = "Kích cỡ", NetworkId = "SIZE", Remark = "Loại quy cách 1 — kích cỡ" },
                new SpecType { Kind = 1, Code = "CHATLIEU", Name = "Chất liệu", NetworkId = "CHATLIEU" },
                new SpecType { Kind = 2, Code = "MAU", Name = "Màu sắc", NetworkId = "MAU", Remark = "Loại quy cách 2 — màu sắc" },
                new SpecType { Kind = 2, Code = "KIEUDANG", Name = "Kiểu dáng", NetworkId = "KIEUDANG" });
            await db.SaveChangesAsync();
        }
        if (!await db.SsccTypes.AnyAsync())
        {
            db.SsccTypes.AddRange(
                new SsccType { Code = "SSCC-THUNG", Name = "Thùng carton", NetworkId = "SSCC-THUNG" },
                new SsccType { Code = "SSCC-PALLET", Name = "Pallet", NetworkId = "SSCC-PALLET" },
                new SsccType { Code = "SSCC-BAO", Name = "Bao / túi", NetworkId = "SSCC-BAO" });
            await db.SaveChangesAsync();
        }
        // Ảnh hàng hóa (Mst_ProductImages) — mỗi hàng hóa nhiều ảnh, 1 ảnh chính.
        if (!await db.ProductImages.AnyAsync())
        {
            db.ProductImages.AddRange(
                new ProductImage { ProductCode = "AO-001", Idx = 0, ImagePath = "/images/ao-001-1.jpg", ImageName = "ao-001-1.jpg", ImageDesc = "Ảnh chính áo sơ mi trắng", FlagPrimaryImage = true },
                new ProductImage { ProductCode = "AO-001", Idx = 1, ImagePath = "/images/ao-001-2.jpg", ImageName = "ao-001-2.jpg", ImageDesc = "Ảnh mặt sau" },
                new ProductImage { ProductCode = "QUAN-001", Idx = 0, ImagePath = "/images/quan-001-1.jpg", ImageName = "quan-001-1.jpg", ImageDesc = "Ảnh chính quần jeans", FlagPrimaryImage = true });
            await db.SaveChangesAsync();
        }
        // File đính kèm hàng hóa (Mst_ProductFiles) — bản vẽ, hướng dẫn, chứng từ.
        if (!await db.ProductFiles.AnyAsync())
        {
            db.ProductFiles.AddRange(
                new ProductFile { ProductCode = "AO-001", Idx = 0, FilePath = "/files/ao-001-spec.pdf", FileName = "ao-001-spec.pdf", FileDesc = "Bản vẽ kỹ thuật" },
                new ProductFile { ProductCode = "QUAN-001", Idx = 0, FilePath = "/files/quan-001-guide.pdf", FileName = "quan-001-guide.pdf", FileDesc = "Hướng dẫn sử dụng" });
            await db.SaveChangesAsync();
        }
        // Trường động của Hàng hóa (Product_CustomField) — định nghĩa trường động gắn cho Hàng hóa master.
        if (!await db.ProductCustomFields.AnyAsync())
        {
            db.ProductCustomFields.AddRange(
                new ProductCustomField { Code = "PCF001", Name = "Mã vạch phụ", DBPhysicalType = "nvarchar(400)", NetworkId = "PCF001" },
                new ProductCustomField { Code = "PCF002", Name = "Nhóm thuế đặc biệt", DBPhysicalType = "nvarchar(400)" },
                new ProductCustomField { Code = "PCF003", Name = "Khối lượng tịnh (g)", DBPhysicalType = "decimal(18,3)" });
            await db.SaveChangesAsync();
        }
        // Ảnh quy cách (Mst_SpecImage) — mỗi quy cách nhiều ảnh, 1 ảnh chính.
        if (!await db.SpecImages.AnyAsync())
        {
            db.SpecImages.AddRange(
                new SpecImage { SpecCode = "AO-001-DENIM-M", ImagePath = "/images/spec-ao-001-1.jpg", ImageName = "spec-ao-001-1.jpg", ImageDesc = "Ảnh chính quy cách áo sơ mi", FlagPrimaryImage = true },
                new SpecImage { SpecCode = "AO-001-DENIM-M", ImagePath = "/images/spec-ao-001-2.jpg", ImageName = "spec-ao-001-2.jpg", ImageDesc = "Ảnh mặt sau" },
                new SpecImage { SpecCode = "QUAN-001-DENIM-32", ImagePath = "/images/spec-quan-001-1.jpg", ImageName = "spec-quan-001-1.jpg", ImageDesc = "Ảnh chính quy cách quần jeans", FlagPrimaryImage = true });
            await db.SaveChangesAsync();
        }
        // File đính kèm quy cách (Mst_SpecFiles) — bản vẽ, hướng dẫn, chứng từ.
        if (!await db.SpecFiles.AnyAsync())
        {
            db.SpecFiles.AddRange(
                new SpecFile { SpecCode = "AO-001-DENIM-M", FilePath = "/files/spec-ao-001-spec.pdf", FileName = "spec-ao-001-spec.pdf", FileDesc = "Bản vẽ kỹ thuật quy cách" },
                new SpecFile { SpecCode = "QUAN-001-DENIM-32", FilePath = "/files/spec-quan-001-guide.pdf", FileName = "spec-quan-001-guide.pdf", FileDesc = "Hướng dẫn sử dụng" });
            await db.SaveChangesAsync();
        }
    }

    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Groups", "Products", "Attributes", "BomLines", "AttributeDefs", "Specs", "SpecPrices", "Units", "VatRates", "Brands", "Models", "ProductTypes", "CurrencyExes", "SpecUnits", "SpecCustomFields", "SpecTypes", "SsccTypes", "ProductImages", "ProductFiles", "ProductCustomFields", "SpecImages", "SpecFiles" };
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
        // Loại SSCC (Mst_SSCCType): cột tham chiếu + GTIN trên Hàng hóa.
        sql.Add("ALTER TABLE minipim.\"Products\" ADD COLUMN IF NOT EXISTS \"SsccTypeCode\" text NULL");
        sql.Add("ALTER TABLE minipim.\"Products\" ADD COLUMN IF NOT EXISTS \"Gtin\" text NULL");
        // Vòng đời hàng hóa (Mst_Product_UpdateDtimeUsed): ngày ngừng sử dụng.
        sql.Add("ALTER TABLE minipim.\"Products\" ADD COLUMN IF NOT EXISTS \"DTimeUsed\" timestamp NULL");
        // Phân cấp hàng hóa (Mst_Product): mã gốc / mã cơ sở.
        sql.Add("ALTER TABLE minipim.\"Products\" ADD COLUMN IF NOT EXISTS \"ProductCodeRoot\" text NULL");
        sql.Add("ALTER TABLE minipim.\"Products\" ADD COLUMN IF NOT EXISTS \"ProductCodeBase\" text NULL");
        // Mã hàng hóa người dùng (Mst_Product.ProductCodeUser).
        sql.Add("ALTER TABLE minipim.\"Products\" ADD COLUMN IF NOT EXISTS \"CodeUser\" text NULL");
        // Thuộc tính (Mst_Attribute): mã dùng chung network.
        sql.Add("ALTER TABLE minipim.\"AttributeDefs\" ADD COLUMN IF NOT EXISTS \"NetworkId\" text NULL");
        sql.Add("ALTER TABLE minipim.\"AttributeDefs\" ADD COLUMN IF NOT EXISTS \"UpdatedAt\" timestamp NOT NULL DEFAULT now()");
        // Ảnh hàng hóa (Mst_ProductImages) — bảng mới.
        sql.Add("CREATE TABLE IF NOT EXISTS minipim.\"ProductImages\" (\"Id\" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"ProductCode\" text NOT NULL DEFAULT '', \"Idx\" integer NOT NULL DEFAULT 0, \"NetworkId\" text NULL, \"ImagePath\" text NULL, \"ImageName\" text NULL, \"ImageDesc\" text NULL, \"FlagPrimaryImage\" boolean NOT NULL DEFAULT false, \"Active\" boolean NOT NULL DEFAULT true, \"UpdatedAt\" timestamp NOT NULL DEFAULT now())");
        // File đính kèm hàng hóa (Mst_ProductFiles) — bảng mới.
        sql.Add("CREATE TABLE IF NOT EXISTS minipim.\"ProductFiles\" (\"Id\" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"ProductCode\" text NOT NULL DEFAULT '', \"Idx\" integer NOT NULL DEFAULT 0, \"NetworkId\" text NULL, \"FilePath\" text NULL, \"FileName\" text NULL, \"FileDesc\" text NULL, \"Active\" boolean NOT NULL DEFAULT true, \"UpdatedAt\" timestamp NOT NULL DEFAULT now())");
        // Trường động của Hàng hóa (Product_CustomField) — bảng mới.
        sql.Add("CREATE TABLE IF NOT EXISTS minipim.\"ProductCustomFields\" (\"Id\" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"Code\" text NOT NULL DEFAULT '', \"Name\" text NOT NULL DEFAULT '', \"NetworkId\" text NULL, \"DBPhysicalType\" text NULL, \"Active\" boolean NOT NULL DEFAULT true, \"UpdatedAt\" timestamp NOT NULL DEFAULT now())");
        // Ảnh quy cách (Mst_SpecImage) — bảng mới.
        sql.Add("CREATE TABLE IF NOT EXISTS minipim.\"SpecImages\" (\"Id\" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"SpecCode\" text NOT NULL DEFAULT '', \"NetworkId\" text NULL, \"ImagePath\" text NULL, \"ImageName\" text NULL, \"ImageDesc\" text NULL, \"FlagPrimaryImage\" boolean NOT NULL DEFAULT false, \"Active\" boolean NOT NULL DEFAULT true, \"UpdatedAt\" timestamp NOT NULL DEFAULT now())");
        // File đính kèm quy cách (Mst_SpecFiles) — bảng mới.
        sql.Add("CREATE TABLE IF NOT EXISTS minipim.\"SpecFiles\" (\"Id\" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"SpecCode\" text NOT NULL DEFAULT '', \"NetworkId\" text NULL, \"FilePath\" text NULL, \"FileName\" text NULL, \"FileDesc\" text NULL, \"Active\" boolean NOT NULL DEFAULT true, \"UpdatedAt\" timestamp NOT NULL DEFAULT now())");
        foreach (var s in sql) try { await db.Database.ExecuteSqlRawAsync(s); } catch { }
    }
}
