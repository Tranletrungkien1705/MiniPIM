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
            // Danh mục chuẩn phụ tùng ô tô — nguồn master cấp cho MiniWMS/MiniDMS/MiniStamp.
            db.Groups.AddRange(
                new ProductGroup { Code = "BDG", Name = "Bảo dưỡng" },
                new ProductGroup { Code = "PHANH", Name = "Hệ thống phanh" },
                new ProductGroup { Code = "DIEN", Name = "Hệ thống điện" },
                new ProductGroup { Code = "DONGCO", Name = "Động cơ" },
                new ProductGroup { Code = "LOP", Name = "Lốp & vành" });
            await db.SaveChangesAsync();
        }
        if (!await db.Products.AnyAsync())
        {
            var groups = await db.Groups.ToListAsync();
            int GId(string c) => groups.First(g => g.Code == c).Id;
            var items = new (string code, string name, string grp, string uom, decimal cost, decimal sale, (string k, string v)[] attrs)[]
            {
                ("PT-LOC-DAU", "Lọc dầu động cơ", "BDG", "cái", 85_000, 150_000, new[]{("Hãng","Hyundai Mobis"),("Mã OEM","26300-35505"),("Xuất xứ","Hàn Quốc")}),
                ("PT-LOC-GIO", "Lọc gió động cơ", "BDG", "cái", 120_000, 220_000, new[]{("Hãng","Hyundai Mobis"),("Mã OEM","28113-2S000")}),
                ("PT-DAU-NHOT", "Dầu nhớt 5W-30 (1L)", "BDG", "lít", 130_000, 210_000, new[]{("Cấp","API SN"),("Độ nhớt","5W-30")}),
                ("PT-BUGI", "Bugi đánh lửa", "DIEN", "cái", 90_000, 160_000, new[]{("Loại","Iridium"),("Khe hở","1.0mm")}),
                ("PT-ACQUY", "Ắc quy 12V-60Ah", "DIEN", "cái", 1_350_000, 1_950_000, new[]{("Điện áp","12V"),("Dung lượng","60Ah")}),
                ("PT-BONG-DEN", "Bóng đèn pha H4", "DIEN", "cái", 110_000, 200_000, new[]{("Chuẩn","H4"),("Công suất","60/55W")}),
                ("PT-MA-PHANH-TR", "Má phanh trước", "PHANH", "bộ", 480_000, 780_000, new[]{("Vị trí","Trước"),("Chất liệu","Ceramic")}),
                ("PT-DIA-PHANH", "Đĩa phanh trước", "PHANH", "cái", 950_000, 1_450_000, new[]{("Đường kính","305mm")}),
                ("PT-DAY-CUROA", "Dây curoa tổng", "DONGCO", "cái", 380_000, 620_000, new[]{("Số rãnh","6PK")}),
                ("PT-LOP", "Lốp 215/60R17", "LOP", "cái", 1_850_000, 2_650_000, new[]{("Kích thước","215/60R17"),("Hãng","Michelin")}),
            };
            foreach (var it in items)
                db.Products.Add(new Product
                {
                    Code = it.code, Name = it.name, GroupId = GId(it.grp), Uom = it.uom, CostPrice = it.cost, SalePrice = it.sale,
                    Barcode = "893" + Math.Abs(it.code.GetHashCode()).ToString().PadLeft(10, '0')[..10],
                    Description = $"Phụ tùng chính hãng — {it.name}",
                    Attributes = it.attrs.Select(a => new ProductAttribute { Name = a.k, Value = a.v }).ToList()
                });
            // 1 sản phẩm có BOM (bộ bảo dưỡng cấp 1 = gói combo).
            db.Products.Add(new Product
            {
                Code = "COMBO-BD1", Name = "Gói bảo dưỡng cấp 1", GroupId = GId("BDG"), Uom = "gói", CostPrice = 300_000, SalePrice = 520_000,
                Description = "Gói combo bảo dưỡng định kỳ 5.000km",
                Bom =
                [
                    new() { ComponentCode = "PT-LOC-DAU", ComponentName = "Lọc dầu động cơ", Quantity = 1, Uom = "cái" },
                    new() { ComponentCode = "PT-DAU-NHOT", ComponentName = "Dầu nhớt 5W-30", Quantity = 4, Uom = "lít" },
                    new() { ComponentCode = "PT-LOC-GIO", ComponentName = "Lọc gió động cơ", Quantity = 1, Uom = "cái" },
                ]
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Groups", "Products", "Attributes", "BomLines" };
        var sql = new List<string>
        {
            "CREATE TABLE IF NOT EXISTS minipim.\"Orgs\" (\"Id\" uuid PRIMARY KEY, \"Name\" text NOT NULL DEFAULT '', \"ApiKey\" text NOT NULL DEFAULT '', \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Orgs_ApiKey\" ON minipim.\"Orgs\" (\"ApiKey\")",
        };
        foreach (var t in tables) sql.Add($"ALTER TABLE minipim.\"{t}\" ADD COLUMN IF NOT EXISTS \"OrgId\" uuid NOT NULL DEFAULT '{def}'");
        foreach (var s in sql) try { await db.Database.ExecuteSqlRawAsync(s); } catch { }
    }
}
