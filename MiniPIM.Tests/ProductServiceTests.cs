using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniPIM.Data;
using MiniPIM.Models;
using MiniPIM.Services;
using Xunit;

namespace MiniPIM.Tests;

/// <summary>Test PIM: lưu SP kèm thuộc tính động + BOM, sửa thay thế attrs/bom, tìm kiếm, dashboard theo nhóm.</summary>
public class ProductServiceTests
{
    private static (AppDbContext db, IProductService svc, SqliteConnection conn) NewSvc()
    {
        var conn = new SqliteConnection("DataSource=:memory:"); conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var db = new AppDbContext(opt, new TenantContext { OrgId = TenantContext.DefaultOrgId });
        db.Database.EnsureCreated();
        return (db, new ProductService(db), conn);
    }

    [Fact]
    public async Task Save_NewProduct_WithAttrsAndBom()
    {
        var (db, svc, conn) = NewSvc(); using (conn)
        {
            var id = await svc.SaveProductAsync(
                new Product { Code = "P1", Name = "Combo BD", SalePrice = 500 },
                new() { new() { Name = "Hãng", Value = "Mobis" } },
                new() { new() { ComponentName = "Lọc dầu", Quantity = 1 }, new() { ComponentName = "Nhớt", Quantity = 4 } });
            var p = await svc.GetAsync(id);
            Assert.Single(p!.Attributes);
            Assert.Equal(2, p.Bom.Count);
        }
    }

    [Fact]
    public async Task AutoCode_WhenBlank()
    {
        var (db, svc, conn) = NewSvc(); using (conn)
        {
            var id = await svc.SaveProductAsync(new Product { Name = "X" }, new(), new());
            var p = await svc.GetAsync(id);
            Assert.StartsWith("SP", p!.Code);
        }
    }

    [Fact]
    public async Task Edit_ReplacesAttributesAndBom()
    {
        var (db, svc, conn) = NewSvc(); using (conn)
        {
            var id = await svc.SaveProductAsync(new Product { Code = "P1", Name = "A" },
                new() { new() { Name = "Cũ", Value = "1" } }, new() { new() { ComponentName = "cũ" } });
            await svc.SaveProductAsync(new Product { Id = id, Code = "P1", Name = "A2" },
                new() { new() { Name = "Mới", Value = "2" } }, new());
            var p = await svc.GetAsync(id);
            Assert.Equal("A2", p!.Name);
            Assert.Single(p.Attributes);
            Assert.Equal("Mới", p.Attributes[0].Name);
            Assert.Empty(p.Bom);   // BOM cũ bị xóa
        }
    }

    [Fact]
    public async Task Search_ByCodeNameBarcode()
    {
        var (db, svc, conn) = NewSvc(); using (conn)
        {
            await svc.SaveProductAsync(new Product { Code = "LOC-DAU", Name = "Lọc dầu", Barcode = "8931" }, new(), new());
            await svc.SaveProductAsync(new Product { Code = "BUGI", Name = "Bugi" }, new(), new());
            Assert.Single(await svc.ProductsAsync("dầu", null));
            Assert.Single(await svc.ProductsAsync("8931", null));
            Assert.Equal(2, (await svc.ProductsAsync(null, null)).Count);
        }
    }

    [Fact]
    public async Task GetByCode_Works()
    {
        var (db, svc, conn) = NewSvc(); using (conn)
        {
            await svc.SaveProductAsync(new Product { Code = "ABC", Name = "SP" }, new(), new());
            var p = await svc.GetByCodeAsync("ABC");
            Assert.NotNull(p);
            Assert.Equal("SP", p!.Name);
        }
    }

    [Fact]
    public async Task Dashboard_CountsByGroupAndBom()
    {
        var (db, svc, conn) = NewSvc(); using (conn)
        {
            var g = await svc.CreateGroupAsync(new ProductGroup { Code = "G1", Name = "Nhóm 1" });
            await svc.SaveProductAsync(new Product { Code = "A", Name = "A", GroupId = g }, new(), new() { new() { ComponentName = "x" } });
            await svc.SaveProductAsync(new Product { Code = "B", Name = "B", GroupId = g }, new(), new());
            var d = await svc.DashboardAsync();
            Assert.Equal(2, d.Products);
            Assert.Equal(1, d.WithBom);
            Assert.Equal(1, d.Groups);
            Assert.Contains(d.ByGroup, x => x.Group == "Nhóm 1" && x.Count == 2);
        }
    }
}
