using Microsoft.EntityFrameworkCore;
using MiniPIM.Data;
using MiniPIM.Models;

namespace MiniPIM.Services;

public record PimDash(int Products, int Active, int Groups, int WithBom, List<(string Group, int Count)> ByGroup);

public interface IProductService
{
    Task<List<Product>> ProductsAsync(string? q, int? groupId);
    Task<Product?> GetAsync(int id);
    Task<Product?> GetByCodeAsync(string code);
    Task<List<ProductGroup>> GroupsAsync();
    Task<int> SaveProductAsync(Product p, List<ProductAttribute> attrs, List<BomLine> bom);
    Task<string?> ValidateBomAsync(List<BomLine> bom);
    Task<int> CreateGroupAsync(ProductGroup g);
    Task<List<AttributeDef>> AttributeDefsAsync();
    Task<int> CreateAttributeDefAsync(AttributeDef a);
    Task<PimDash> DashboardAsync();
}

public class ProductService(AppDbContext db) : IProductService
{
    public async Task<List<Product>> ProductsAsync(string? q, int? groupId)
    {
        var query = db.Products.Include(p => p.Group).AsQueryable();
        if (groupId.HasValue) query = query.Where(p => p.GroupId == groupId.Value);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Name.Contains(q) || p.Code.Contains(q) || (p.Barcode ?? "").Contains(q));
        return await query.OrderBy(p => p.Code).ToListAsync();
    }

    public Task<Product?> GetAsync(int id) =>
        db.Products.Include(p => p.Group).Include(p => p.Attributes).Include(p => p.Bom).FirstOrDefaultAsync(p => p.Id == id);

    public Task<Product?> GetByCodeAsync(string code) =>
        db.Products.Include(p => p.Group).Include(p => p.Attributes).Include(p => p.Bom).FirstOrDefaultAsync(p => p.Code == code);

    public Task<List<ProductGroup>> GroupsAsync() => db.Groups.OrderBy(g => g.Name).ToListAsync();

    public async Task<int> SaveProductAsync(Product p, List<ProductAttribute> attrs, List<BomLine> bom)
    {
        Product target;
        if (p.Id > 0)
        {
            target = await db.Products.Include(x => x.Attributes).Include(x => x.Bom).FirstAsync(x => x.Id == p.Id);
            target.Name = p.Name; target.GroupId = p.GroupId; target.Uom = p.Uom; target.Barcode = p.Barcode;
            target.CostPrice = p.CostPrice; target.SalePrice = p.SalePrice; target.ImageUrl = p.ImageUrl;
            target.Description = p.Description; target.Status = p.Status; target.UpdatedAt = DateTime.Now;
            target.Level = p.Level; target.ValConvert = p.ValConvert; target.QtyMinSt = p.QtyMinSt; target.QtyMaxSt = p.QtyMaxSt;
            target.VatRateCode = p.VatRateCode; target.FlagSerial = p.FlagSerial; target.FlagLot = p.FlagLot;
            target.Origin = p.Origin; target.QuyCach = p.QuyCach;
            db.Attributes.RemoveRange(target.Attributes);
            db.BomLines.RemoveRange(target.Bom);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(p.Code)) p.Code = $"SP{await db.Products.CountAsync() + 1:D4}";
            target = p;
            db.Products.Add(target);
        }
        target.Attributes = attrs.Where(a => !string.IsNullOrWhiteSpace(a.Name)).ToList();
        target.Bom = bom.Where(x => !string.IsNullOrWhiteSpace(x.ComponentName)).ToList();
        await db.SaveChangesAsync();
        return target.Id;
    }

    public async Task<string?> ValidateBomAsync(List<BomLine> bom)
    {
        // Nghiệp vụ ProductCenter: thành phần trong BOM không được quản lý serial/lô.
        var codes = bom.Where(x => !string.IsNullOrWhiteSpace(x.ComponentCode)).Select(x => x.ComponentCode).Distinct().ToList();
        if (codes.Count == 0) return null;
        var bad = await db.Products.Where(p => codes.Contains(p.Code) && (p.FlagSerial || p.FlagLot))
            .Select(p => p.Code).ToListAsync();
        return bad.Count == 0 ? null : $"Thành phần BOM không được quản lý serial/lô: {string.Join(", ", bad)}";
    }

    public async Task<int> CreateGroupAsync(ProductGroup g)
    {
        if (string.IsNullOrWhiteSpace(g.Code)) g.Code = $"G{await db.Groups.CountAsync() + 1:D2}";
        db.Groups.Add(g);
        await db.SaveChangesAsync();
        return g.Id;
    }

    public Task<List<AttributeDef>> AttributeDefsAsync() => db.AttributeDefs.OrderBy(a => a.Name).ToListAsync();

    public async Task<int> CreateAttributeDefAsync(AttributeDef a)
    {
        if (string.IsNullOrWhiteSpace(a.Code)) a.Code = $"AT{await db.AttributeDefs.CountAsync() + 1:D2}";
        db.AttributeDefs.Add(a);
        await db.SaveChangesAsync();
        return a.Id;
    }

    public async Task<PimDash> DashboardAsync()
    {
        var products = await db.Products.Include(p => p.Group).Include(p => p.Bom).ToListAsync();
        var byGroup = products.GroupBy(p => p.Group?.Name ?? "(chưa nhóm)").Select(gr => (gr.Key, gr.Count()))
            .OrderByDescending(x => x.Item2).Take(6).ToList();
        return new PimDash(products.Count, products.Count(p => p.Status == ProductStatus.Active),
            await db.Groups.CountAsync(), products.Count(p => p.Bom.Count > 0), byGroup);
    }
}
