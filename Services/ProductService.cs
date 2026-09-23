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

    // --- Nhóm hàng phân cấp (Mst_ProductGroup) ---
    Task<ProductGroup?> GetGroupAsync(int id);
    Task<string?> SaveGroupAsync(ProductGroup g);
    Task<string?> DeleteGroupAsync(int id);
    Task<List<AttributeDef>> AttributeDefsAsync(string? q);
    Task<AttributeDef?> GetAttributeDefAsync(int id);
    Task<string?> SaveAttributeDefAsync(AttributeDef a);
    Task<string?> DeleteAttributeDefAsync(int id);
    Task<PimDash> DashboardAsync();

    // --- Đơn vị tính (Mst_Unit) ---
    Task<List<Unit>> UnitsAsync(string? q);
    Task<Unit?> GetUnitAsync(int id);
    Task<string?> SaveUnitAsync(Unit u);
    Task<string?> DeleteUnitAsync(int id);

    // --- Quy cách (Mst_Spec) & bảng giá theo quy cách (Mst_SpecPrice) ---
    Task<List<Spec>> SpecsAsync(string? q);
    Task<Spec?> GetSpecAsync(int id);
    Task<int> SaveSpecAsync(Spec s);
    Task<List<SpecPrice>> SpecPricesAsync(string? specCode);
    Task<string?> SaveSpecPriceAsync(SpecPrice p);

    // --- Thuế suất GTGT (Mst_VATRate) ---
    Task<List<VatRate>> VatRatesAsync(string? q);
    Task<VatRate?> GetVatRateAsync(int id);
    Task<string?> SaveVatRateAsync(VatRate v);
    Task<string?> DeleteVatRateAsync(int id);

    // --- Nhãn hiệu (Mst_Brand) ---
    Task<List<Brand>> BrandsAsync(string? q);
    Task<Brand?> GetBrandAsync(int id);
    Task<string?> SaveBrandAsync(Brand b);
    Task<string?> DeleteBrandAsync(int id);

    // --- Model / Dòng sản phẩm (Mst_Model) ---
    Task<List<Model>> ModelsAsync(string? q);
    Task<Model?> GetModelAsync(int id);
    Task<string?> SaveModelAsync(Model m);
    Task<string?> DeleteModelAsync(int id);

    // --- Loại hàng hóa (Mst_ProductType) ---
    Task<List<ProductType>> ProductTypesAsync(string? q);
    Task<ProductType?> GetProductTypeAsync(int id);
    Task<string?> SaveProductTypeAsync(ProductType t);
    Task<string?> DeleteProductTypeAsync(int id);

    // --- Tỷ giá ngoại tệ (Mst_CurrencyEx) ---
    Task<List<CurrencyEx>> CurrencyExesAsync(string? q);
    Task<CurrencyEx?> GetCurrencyExAsync(int id);
    Task<string?> SaveCurrencyExAsync(CurrencyEx c);
    Task<string?> DeleteCurrencyExAsync(int id);

    // --- Đơn vị tính theo quy cách (Mst_SpecUnit) ---
    Task<List<SpecUnit>> SpecUnitsAsync(string? specCode);
    Task<SpecUnit?> GetSpecUnitAsync(int id);
    Task<string?> SaveSpecUnitAsync(SpecUnit u);
    Task<string?> DeleteSpecUnitAsync(int id);

    // --- Trường tùy chỉnh quy cách (Mst_SpecCustomField) ---
    Task<List<SpecCustomField>> SpecCustomFieldsAsync(string? q);
    Task<SpecCustomField?> GetSpecCustomFieldAsync(int id);
    Task<string?> SaveSpecCustomFieldAsync(SpecCustomField f);
    Task<string?> DeleteSpecCustomFieldAsync(int id);

    // --- Loại quy cách (Mst_SpecType1 / Mst_SpecType2) ---
    Task<List<SpecType>> SpecTypesAsync(int? kind, string? q);
    Task<SpecType?> GetSpecTypeAsync(int id);
    Task<string?> SaveSpecTypeAsync(SpecType t);
    Task<string?> DeleteSpecTypeAsync(int id);
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

    // --- Nhóm hàng phân cấp (Mst_ProductGroup) ---
    public Task<ProductGroup?> GetGroupAsync(int id) =>
        db.Groups.FirstOrDefaultAsync(g => g.Id == id);

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_ProductGroup_CreateX / UpdateX):
    /// mã & tên nhóm bắt buộc, mã/tên không trùng, nhóm cha phải tồn tại.
    /// Sau khi lưu, tính lại đường dẫn BU (Mst_ProductGroup_UpdBU).
    /// Trả về thông báo lỗi hoặc null nếu OK.
    /// </summary>
    public async Task<string?> SaveGroupAsync(ProductGroup g)
    {
        if (string.IsNullOrWhiteSpace(g.Code)) return "Mã Nhóm hàng không hợp lệ.";
        if (string.IsNullOrWhiteSpace(g.Name)) return "Tên Nhóm hàng không hợp lệ.";
        g.Code = g.Code.Trim(); g.Name = g.Name.Trim();

        var dupCode = await db.Groups.AnyAsync(x => x.Code == g.Code && x.Id != g.Id);
        if (dupCode) return $"Nhóm hàng '{g.Code}' đã tồn tại.";
        var dupName = await db.Groups.AnyAsync(x => x.Name == g.Name && x.Id != g.Id);
        if (dupName) return $"Tên nhóm hàng '{g.Name}' đã tồn tại.";

        if (!string.IsNullOrWhiteSpace(g.ParentCode))
        {
            var parent = await db.Groups.FirstOrDefaultAsync(x => x.Code == g.ParentCode);
            if (parent == null) return $"Nhóm cha '{g.ParentCode}' không tồn tại.";
            if (g.Id > 0 && parent.Id == g.Id) return "Nhóm không thể là cha của chính nó.";
        }

        ProductGroup target;
        if (g.Id > 0)
        {
            target = await db.Groups.FirstAsync(x => x.Id == g.Id);
            target.Name = g.Name; target.ParentCode = g.ParentCode; target.BrandCode = g.BrandCode;
            target.FlagFG = g.FlagFG; target.Active = g.Active; target.UpdatedAt = DateTime.Now;
        }
        else
        {
            target = g;
            db.Groups.Add(target);
        }
        await db.SaveChangesAsync();
        await RecomputeGroupPathsAsync();
        return null;
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_ProductGroup_DeleteX): không cho xóa nhóm
    /// nếu còn hàng hóa thuộc nhóm hoặc còn nhóm con.
    /// </summary>
    public async Task<string?> DeleteGroupAsync(int id)
    {
        var g = await db.Groups.FirstOrDefaultAsync(x => x.Id == id);
        if (g == null) return "Không tìm thấy thông tin Nhóm hàng.";
        if (await db.Products.AnyAsync(p => p.GroupId == id))
            return $"Đã có Hàng hóa trong Nhóm hàng '{g.Name}'.";
        if (await db.Groups.AnyAsync(x => x.ParentCode == g.Code))
            return $"Nhóm hàng '{g.Name}' còn nhóm con, không thể xóa.";
        db.Groups.Remove(g);
        await db.SaveChangesAsync();
        await RecomputeGroupPathsAsync();
        return null;
    }

    /// <summary>
    /// Port Mst_ProductGroup_UpdBU: duyệt cây cha/con và gán lại BUCode/BUPattern/Level.
    /// Gốc (không có cha) có BUCode = mã, Level = 0; con = "cha.con", Level = cha.Level + 1.
    /// </summary>
    private async Task RecomputeGroupPathsAsync()
    {
        var groups = await db.Groups.ToListAsync();
        var byCode = groups.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

        string Path(ProductGroup g, int depth)
        {
            if (depth > 32) return g.Code; // chống vòng lặp
            if (string.IsNullOrWhiteSpace(g.ParentCode) || !byCode.TryGetValue(g.ParentCode, out var parent))
                return g.Code;
            return Path(parent, depth + 1) + "." + g.Code;
        }
        int Level(ProductGroup g, int depth)
        {
            if (depth > 32) return depth;
            if (string.IsNullOrWhiteSpace(g.ParentCode) || !byCode.TryGetValue(g.ParentCode, out var parent))
                return 0;
            return Level(parent, depth + 1) + 1;
        }

        foreach (var g in groups)
        {
            g.BUCode = Path(g, 0);
            g.BUPattern = g.BUCode + "%";
            g.Level = Level(g, 0);
        }
        await db.SaveChangesAsync();
    }

    // --- Thuộc tính dùng chung (Mst_Attribute) ---
    public async Task<List<AttributeDef>> AttributeDefsAsync(string? q)
    {
        var query = db.AttributeDefs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(a => a.Code.Contains(q) || a.Name.Contains(q) || (a.NetworkId ?? "").Contains(q));
        return await query.OrderBy(a => a.Name).ToListAsync();
    }

    public Task<AttributeDef?> GetAttributeDefAsync(int id) => db.AttributeDefs.FirstOrDefaultAsync(a => a.Id == id);

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_Attribute_CreateX / Mst_Attribute_UpdateX / Mst_Attribute_CheckDB /
    /// Mst_Attribute_CheckAttributeName): mã thuộc tính bắt buộc (Mst_Attribute_Create_InvalidAttributeCode),
    /// mã không trùng trong tổ chức (Mst_Attribute_CheckDB_AttributeExist), tên thuộc tính bắt buộc
    /// (Mst_Attribute_Create_InvalidAttributeName / Mst_Attribute_UpdateX_InvalidAttributeName),
    /// tên không trùng trong cùng NetworkID (Mst_Attribute_CheckDB_AttributeExist), khi sửa/xóa phải tồn tại
    /// (Mst_Attribute_CheckDB_AttributeNotFound). Trả về thông báo lỗi hoặc null nếu OK.
    /// </summary>
    public async Task<string?> SaveAttributeDefAsync(AttributeDef a)
    {
        if (string.IsNullOrWhiteSpace(a.Code)) return "Mã Đặc tính Attribute không hợp lệ.";
        if (string.IsNullOrWhiteSpace(a.Name)) return "Tên Đặc tính Attribute không hợp lệ.";
        a.Code = a.Code.Trim(); a.Name = a.Name.Trim();
        a.NetworkId = string.IsNullOrWhiteSpace(a.NetworkId) ? null : a.NetworkId.Trim();

        // Mst_Attribute_CheckDB: mã không trùng trong tổ chức.
        var dupCode = await db.AttributeDefs.AnyAsync(x => x.Code == a.Code && x.Id != a.Id);
        if (dupCode) return $"Mã Đặc tính Attribute '{a.Code}' đã tồn tại.";

        // Mst_Attribute_CheckAttributeName: tên không trùng trong cùng NetworkID.
        var dupName = await db.AttributeDefs.AnyAsync(x => x.Name == a.Name && x.NetworkId == a.NetworkId && x.Id != a.Id);
        if (dupName) return $"Tên Đặc tính Attribute '{a.Name}' đã tồn tại.";

        AttributeDef target;
        if (a.Id > 0)
        {
            target = await db.AttributeDefs.FirstOrDefaultAsync(x => x.Id == a.Id)
                ?? throw new InvalidOperationException("Không tìm thấy thông tin Đặc tính Attribute của Hàng hóa.");
            target.Name = a.Name; target.NetworkId = a.NetworkId;
            target.Active = a.Active; target.UpdatedAt = DateTime.Now;
        }
        else
        {
            target = a;
            db.AttributeDefs.Add(target);
        }
        await db.SaveChangesAsync();
        return null;
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_Attribute_DeleteX / Mst_Attribute_CheckDB):
    /// thuộc tính phải tồn tại mới cho xóa (Mst_Attribute_CheckDB_AttributeNotFound).
    /// </summary>
    public async Task<string?> DeleteAttributeDefAsync(int id)
    {
        var a = await db.AttributeDefs.FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return "Không tìm thấy thông tin Đặc tính Attribute của Hàng hóa.";
        db.AttributeDefs.Remove(a);
        await db.SaveChangesAsync();
        return null;
    }

    // --- Đơn vị tính (Mst_Unit) ---
    public async Task<List<Unit>> UnitsAsync(string? q)
    {
        var query = db.Units.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u => u.Name.Contains(q) || u.Code.Contains(q) || u.CodeUser.Contains(q));
        return await query.OrderBy(u => u.Name).ToListAsync();
    }

    public Task<Unit?> GetUnitAsync(int id) => db.Units.FirstOrDefaultAsync(u => u.Id == id);

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_Unit_CreateX / Mst_Unit_UpdateX):
    /// mã người dùng & tên ĐVT bắt buộc, không trùng trong tổ chức; mã hệ thống tự sinh.
    /// Trả về thông báo lỗi hoặc null nếu OK.
    /// </summary>
    public async Task<string?> SaveUnitAsync(Unit u)
    {
        if (string.IsNullOrWhiteSpace(u.Name)) return "Tên Đơn vị tính không hợp lệ.";
        u.Name = u.Name.Trim();
        u.CodeUser = (u.CodeUser ?? "").Trim();
        if (string.IsNullOrWhiteSpace(u.CodeUser)) return "Mã Đơn vị tính không hợp lệ.";

        var dupName = await db.Units.AnyAsync(x => x.Name == u.Name && x.Id != u.Id);
        if (dupName) return $"Tên Đơn vị tính '{u.Name}' đã tồn tại.";
        var dupCodeUser = await db.Units.AnyAsync(x => x.CodeUser == u.CodeUser && x.Id != u.Id);
        if (dupCodeUser) return $"Mã Đơn vị tính '{u.CodeUser}' đã tồn tại.";

        Unit target;
        if (u.Id > 0)
        {
            target = await db.Units.FirstAsync(x => x.Id == u.Id);
            target.Name = u.Name; target.CodeUser = u.CodeUser;
            target.Remark = u.Remark; target.Active = u.Active; target.UpdatedAt = DateTime.Now;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(u.Code)) u.Code = $"UOM{await db.Units.CountAsync() + 1:D4}";
            target = u;
            db.Units.Add(target);
        }
        await db.SaveChangesAsync();
        return null;
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_Unit_DeleteX): ĐVT phải tồn tại mới cho xóa.
    /// </summary>
    public async Task<string?> DeleteUnitAsync(int id)
    {
        var u = await db.Units.FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return "Không tìm thấy thông tin Đơn vị tính.";
        db.Units.Remove(u);
        await db.SaveChangesAsync();
        return null;
    }

    public async Task<PimDash> DashboardAsync()
    {
        var products = await db.Products.Include(p => p.Group).Include(p => p.Bom).ToListAsync();
        var byGroup = products.GroupBy(p => p.Group?.Name ?? "(chưa nhóm)").Select(gr => (gr.Key, gr.Count()))
            .OrderByDescending(x => x.Item2).Take(6).ToList();
        return new PimDash(products.Count, products.Count(p => p.Status == ProductStatus.Active),
            await db.Groups.CountAsync(), products.Count(p => p.Bom.Count > 0), byGroup);
    }

    // --- Quy cách (Mst_Spec) ---
    public async Task<List<Spec>> SpecsAsync(string? q)
    {
        var query = db.Specs.Include(s => s.Prices).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(s => s.Name.Contains(q) || s.Code.Contains(q) || (s.ModelCode ?? "").Contains(q));
        return await query.OrderBy(s => s.Code).ToListAsync();
    }

    public Task<Spec?> GetSpecAsync(int id) =>
        db.Specs.Include(s => s.Prices).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<int> SaveSpecAsync(Spec s)
    {
        Spec target;
        if (s.Id > 0)
        {
            target = await db.Specs.FirstAsync(x => x.Id == s.Id);
            target.Name = s.Name; target.Description = s.Description; target.ModelCode = s.ModelCode;
            target.SpecType1 = s.SpecType1; target.SpecType2 = s.SpecType2; target.Color = s.Color;
            target.FlagHasSerial = s.FlagHasSerial; target.FlagHasLot = s.FlagHasLot;
            target.DefaultUnitCode = s.DefaultUnitCode; target.StandardUnitCode = s.StandardUnitCode;
            target.Remark = s.Remark; target.Active = s.Active; target.UpdatedAt = DateTime.Now;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(s.Code)) s.Code = $"SPEC{await db.Specs.CountAsync() + 1:D4}";
            target = s;
            db.Specs.Add(target);
        }
        await db.SaveChangesAsync();
        return target.Id;
    }

    // --- Bảng giá theo quy cách (Mst_SpecPrice) ---
    public async Task<List<SpecPrice>> SpecPricesAsync(string? specCode)
    {
        var query = db.SpecPrices.AsQueryable();
        if (!string.IsNullOrWhiteSpace(specCode)) query = query.Where(p => p.SpecCode == specCode);
        return await query.OrderBy(p => p.SpecCode).ThenBy(p => p.UnitCode).ToListAsync();
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_SpecPrice_Create): quy cách phải tồn tại & đang dùng,
    /// và mỗi cặp (SpecCode, UnitCode) chỉ có 1 dòng giá. Trả về thông báo lỗi hoặc null nếu OK.
    /// </summary>
    public async Task<string?> SaveSpecPriceAsync(SpecPrice p)
    {
        if (string.IsNullOrWhiteSpace(p.SpecCode)) return "Cần chọn quy cách.";
        if (string.IsNullOrWhiteSpace(p.UnitCode)) return "Cần nhập đơn vị tính.";
        var spec = await db.Specs.FirstOrDefaultAsync(s => s.Code == p.SpecCode);
        if (spec == null) return $"Quy cách '{p.SpecCode}' không tồn tại.";
        if (!spec.Active) return $"Quy cách '{p.SpecCode}' đã ngừng dùng.";
        if (p.Id == 0 && await db.SpecPrices.AnyAsync(x => x.SpecCode == p.SpecCode && x.UnitCode == p.UnitCode))
            return $"Đã có giá cho quy cách '{p.SpecCode}' / ĐVT '{p.UnitCode}'.";
        if (p.SellPrice < 0 || p.BuyPrice < 0) return "Giá không được âm.";

        if (p.Id > 0)
        {
            var target = await db.SpecPrices.FirstAsync(x => x.Id == p.Id);
            target.BuyPrice = p.BuyPrice; target.SellPrice = p.SellPrice; target.CurrencyCode = p.CurrencyCode;
            target.VatRateCode = p.VatRateCode; target.DiscountVnd = p.DiscountVnd;
            target.EffectDTimeStart = p.EffectDTimeStart; target.EffectDTimeEnd = p.EffectDTimeEnd;
            target.Remark = p.Remark; target.Active = p.Active;
        }
        else
        {
            p.SpecId = spec.Id;
            db.SpecPrices.Add(p);
        }
        await db.SaveChangesAsync();
        return null;
    }

    // --- Thuế suất GTGT (Mst_VATRate) ---
    public async Task<List<VatRate>> VatRatesAsync(string? q)
    {
        var query = db.VatRates.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(v => v.Code.Contains(q) || v.Name.Contains(q));
        return await query.OrderBy(v => v.Code).ToListAsync();
    }

    public Task<VatRate?> GetVatRateAsync(int id) => db.VatRates.FirstOrDefaultAsync(v => v.Id == id);

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_VATRate_Create / Mst_VATRate_Update / Mst_VATRate_CheckDB):
    /// mã thuế suất & tên (VATDesc) bắt buộc, mã không trùng; khi sửa phải tồn tại.
    /// Trả về thông báo lỗi hoặc null nếu OK.
    /// </summary>
    public async Task<string?> SaveVatRateAsync(VatRate v)
    {
        if (string.IsNullOrWhiteSpace(v.Code)) return "Mã Thuế suất không hợp lệ.";
        if (string.IsNullOrWhiteSpace(v.Name)) return "Tên Thuế suất không hợp lệ.";
        v.Code = v.Code.Trim(); v.Name = v.Name.Trim();
        if (v.Rate < 0) return "Giá trị Thuế suất không hợp lệ.";

        var dupCode = await db.VatRates.AnyAsync(x => x.Code == v.Code && x.Id != v.Id);
        if (dupCode) return $"Mã Thuế suất '{v.Code}' đã tồn tại.";

        VatRate target;
        if (v.Id > 0)
        {
            target = await db.VatRates.FirstOrDefaultAsync(x => x.Id == v.Id)
                ?? throw new InvalidOperationException("Không tìm thấy thông tin Thuế suất.");
            target.Name = v.Name; target.Rate = v.Rate; target.Active = v.Active; target.UpdatedAt = DateTime.Now;
        }
        else
        {
            target = v;
            db.VatRates.Add(target);
        }
        await db.SaveChangesAsync();
        return null;
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_VATRate_Delete): thuế suất phải tồn tại mới cho xóa.
    /// </summary>
    public async Task<string?> DeleteVatRateAsync(int id)
    {
        var v = await db.VatRates.FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return "Không tìm thấy thông tin Thuế suất.";
        db.VatRates.Remove(v);
        await db.SaveChangesAsync();
        return null;
    }

    // --- Nhãn hiệu (Mst_Brand) ---
    public async Task<List<Brand>> BrandsAsync(string? q)
    {
        var query = db.Brands.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(b => b.Code.Contains(q) || b.Name.Contains(q));
        return await query.OrderBy(b => b.Code).ToListAsync();
    }

    public Task<Brand?> GetBrandAsync(int id) => db.Brands.FirstOrDefaultAsync(b => b.Id == id);

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_Brand_Create / Mst_Brand_Update / Mst_Brand_CheckDB):
    /// mã nhãn hiệu & tên bắt buộc, mã không trùng; khi sửa phải tồn tại.
    /// Trả về thông báo lỗi hoặc null nếu OK.
    /// </summary>
    public async Task<string?> SaveBrandAsync(Brand b)
    {
        if (string.IsNullOrWhiteSpace(b.Code)) return "Mã Nhãn hiệu không hợp lệ.";
        if (string.IsNullOrWhiteSpace(b.Name)) return "Tên Nhãn hiệu không hợp lệ.";
        b.Code = b.Code.Trim(); b.Name = b.Name.Trim();

        var dupCode = await db.Brands.AnyAsync(x => x.Code == b.Code && x.Id != b.Id);
        if (dupCode) return $"Mã Nhãn hiệu '{b.Code}' đã tồn tại.";

        Brand target;
        if (b.Id > 0)
        {
            target = await db.Brands.FirstOrDefaultAsync(x => x.Id == b.Id)
                ?? throw new InvalidOperationException("Không tìm thấy thông tin Nhãn hiệu.");
            target.Name = b.Name; target.NetworkBrandCode = b.NetworkBrandCode;
            target.Remark = b.Remark; target.Active = b.Active; target.UpdatedAt = DateTime.Now;
        }
        else
        {
            target = b;
            db.Brands.Add(target);
        }
        await db.SaveChangesAsync();
        return null;
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_Brand_DeleteX): nhãn hiệu phải tồn tại mới cho xóa,
    /// và không cho xóa nếu nhãn hiệu đang được dùng trong Nhóm hàng
    /// (Mst_Brand_Delete_BrandInProductGroup).
    /// </summary>
    public async Task<string?> DeleteBrandAsync(int id)
    {
        var b = await db.Brands.FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return "Không tìm thấy thông tin Nhãn hiệu.";
        if (await db.Groups.AnyAsync(g => g.BrandCode == b.Code))
            return $"Nhãn hiệu '{b.Name}' đã sử dụng trong Nhóm hàng, không thể xóa.";
        db.Brands.Remove(b);
        await db.SaveChangesAsync();
        return null;
    }

    // --- Model / Dòng sản phẩm (Mst_Model) ---
    public async Task<List<Model>> ModelsAsync(string? q)
    {
        var query = db.Models.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(m => m.Code.Contains(q) || m.Name.Contains(q) || (m.BrandCode ?? "").Contains(q));
        return await query.OrderBy(m => m.Code).ToListAsync();
    }

    public Task<Model?> GetModelAsync(int id) => db.Models.FirstOrDefaultAsync(m => m.Id == id);

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_Model_Create / Mst_Model_Update / Mst_Model_CheckDB):
    /// mã model & tên model bắt buộc, mã không trùng; nhãn hiệu (BrandCode) bắt buộc và
    /// phải tồn tại & đang dùng (Mst_Brand_CheckDB); khi sửa phải tồn tại.
    /// Trả về thông báo lỗi hoặc null nếu OK.
    /// </summary>
    public async Task<string?> SaveModelAsync(Model m)
    {
        if (string.IsNullOrWhiteSpace(m.Code)) return "Mã Model không hợp lệ.";
        if (string.IsNullOrWhiteSpace(m.Name)) return "Tên Model không hợp lệ.";
        m.Code = m.Code.Trim(); m.Name = m.Name.Trim();

        var dupCode = await db.Models.AnyAsync(x => x.Code == m.Code && x.Id != m.Id);
        if (dupCode) return $"Mã Model '{m.Code}' đã tồn tại.";

        // Mst_Model_Create: BrandCode bắt buộc và phải tồn tại & đang dùng.
        if (string.IsNullOrWhiteSpace(m.BrandCode)) return "Cần chọn Nhãn hiệu cho Model.";
        var brand = await db.Brands.FirstOrDefaultAsync(b => b.Code == m.BrandCode);
        if (brand == null) return $"Nhãn hiệu '{m.BrandCode}' không tồn tại.";
        if (!brand.Active) return $"Nhãn hiệu '{m.BrandCode}' đã ngừng dùng.";

        Model target;
        if (m.Id > 0)
        {
            target = await db.Models.FirstOrDefaultAsync(x => x.Id == m.Id)
                ?? throw new InvalidOperationException("Không tìm thấy thông tin Model.");
            target.Name = m.Name; target.OrgModelCode = m.OrgModelCode; target.BrandCode = m.BrandCode;
            target.NetworkModelCode = m.NetworkModelCode; target.Remark = m.Remark;
            target.Active = m.Active; target.UpdatedAt = DateTime.Now;
        }
        else
        {
            target = m;
            db.Models.Add(target);
        }
        await db.SaveChangesAsync();
        return null;
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_Model_Delete): model phải tồn tại mới cho xóa.
    /// </summary>
    public async Task<string?> DeleteModelAsync(int id)
    {
        var m = await db.Models.FirstOrDefaultAsync(x => x.Id == id);
        if (m == null) return "Không tìm thấy thông tin Model.";
        db.Models.Remove(m);
        await db.SaveChangesAsync();
        return null;
    }

    // --- Loại hàng hóa (Mst_ProductType) ---
    public async Task<List<ProductType>> ProductTypesAsync(string? q)
    {
        var query = db.ProductTypes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.Code.Contains(q) || t.Name.Contains(q));
        return await query.OrderBy(t => t.Code).ToListAsync();
    }

    public Task<ProductType?> GetProductTypeAsync(int id) => db.ProductTypes.FirstOrDefaultAsync(t => t.Id == id);

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_ProductType_Create / Mst_ProductType_UpdateX / Mst_ProductType_CheckDB):
    /// mã loại hàng hóa & tên bắt buộc, mã không trùng; khi sửa phải tồn tại.
    /// Trả về thông báo lỗi hoặc null nếu OK.
    /// </summary>
    public async Task<string?> SaveProductTypeAsync(ProductType t)
    {
        if (string.IsNullOrWhiteSpace(t.Code)) return "Mã Loại Hàng hóa không hợp lệ.";
        if (string.IsNullOrWhiteSpace(t.Name)) return "Tên Loại Hàng hóa không hợp lệ.";
        t.Code = t.Code.Trim(); t.Name = t.Name.Trim();

        var dupCode = await db.ProductTypes.AnyAsync(x => x.Code == t.Code && x.Id != t.Id);
        if (dupCode) return $"Loại Hàng hóa '{t.Code}' đã tồn tại.";

        ProductType target;
        if (t.Id > 0)
        {
            target = await db.ProductTypes.FirstOrDefaultAsync(x => x.Id == t.Id)
                ?? throw new InvalidOperationException("Không tìm thấy thông tin Loại Hàng hóa.");
            target.Name = t.Name; target.Remark = t.Remark; target.Active = t.Active; target.UpdatedAt = DateTime.Now;
        }
        else
        {
            target = t;
            db.ProductTypes.Add(target);
        }
        await db.SaveChangesAsync();
        return null;
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_ProductType_Delete): loại hàng hóa phải tồn tại mới cho xóa,
    /// và không cho xóa nếu đang được hàng hóa sử dụng (Mst_Product.ProductType).
    /// </summary>
    public async Task<string?> DeleteProductTypeAsync(int id)
    {
        var t = await db.ProductTypes.FirstOrDefaultAsync(x => x.Id == id);
        if (t == null) return "Không tìm thấy thông tin Loại Hàng hóa.";
        if (await db.Products.AnyAsync(p => p.ProductTypeCode == t.Code))
            return $"Loại Hàng hóa '{t.Name}' đã sử dụng trong Hàng hóa, không thể xóa.";
        db.ProductTypes.Remove(t);
        await db.SaveChangesAsync();
        return null;
    }

    // --- Tỷ giá ngoại tệ (Mst_CurrencyEx) ---
    public async Task<List<CurrencyEx>> CurrencyExesAsync(string? q)
    {
        var query = db.CurrencyExes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Code.Contains(q) || c.Name.Contains(q) || (c.BaseCurrencyCode ?? "").Contains(q));
        return await query.OrderBy(c => c.Code).ToListAsync();
    }

    public Task<CurrencyEx?> GetCurrencyExAsync(int id) => db.CurrencyExes.FirstOrDefaultAsync(c => c.Id == id);

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_CurrencyEx_Create / Mst_CurrencyEx_Update / Mst_CurrencyEx_CheckDB):
    /// mã ngoại tệ & tên bắt buộc, mã không trùng; đồng tiền gốc (BaseCurrencyCode) nếu có
    /// phải tồn tại (Mst_CurrencyEx_CheckDB_CurrencyCodeNotFound); khi sửa phải tồn tại.
    /// Trả về thông báo lỗi hoặc null nếu OK.
    /// </summary>
    public async Task<string?> SaveCurrencyExAsync(CurrencyEx c)
    {
        if (string.IsNullOrWhiteSpace(c.Code)) return "Mã Ngoại tệ không hợp lệ.";
        if (string.IsNullOrWhiteSpace(c.Name)) return "Tên Ngoại tệ không hợp lệ.";
        c.Code = c.Code.Trim(); c.Name = c.Name.Trim();

        var dupCode = await db.CurrencyExes.AnyAsync(x => x.Code == c.Code && x.Id != c.Id);
        if (dupCode) return $"Mã Ngoại tệ '{c.Code}' đã tồn tại.";

        // Mst_CurrencyEx_Create: BaseCurrencyCode nếu có phải tồn tại.
        if (!string.IsNullOrWhiteSpace(c.BaseCurrencyCode))
        {
            c.BaseCurrencyCode = c.BaseCurrencyCode.Trim();
            if (!await db.CurrencyExes.AnyAsync(x => x.Code == c.BaseCurrencyCode))
                return $"Đồng tiền gốc '{c.BaseCurrencyCode}' không tồn tại.";
        }

        CurrencyEx target;
        if (c.Id > 0)
        {
            target = await db.CurrencyExes.FirstOrDefaultAsync(x => x.Id == c.Id)
                ?? throw new InvalidOperationException("Không tìm thấy thông tin Ngoại tệ.");
            target.Name = c.Name; target.BaseCurrencyCode = c.BaseCurrencyCode;
            target.BuyRate = c.BuyRate; target.SellRate = c.SellRate; target.InterEx = c.InterEx;
            target.Remark = c.Remark; target.Active = c.Active; target.UpdatedAt = DateTime.Now;
        }
        else
        {
            target = c;
            db.CurrencyExes.Add(target);
        }
        await db.SaveChangesAsync();
        return null;
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_CurrencyEx_Delete): ngoại tệ phải tồn tại mới cho xóa.
    /// </summary>
    public async Task<string?> DeleteCurrencyExAsync(int id)
    {
        var c = await db.CurrencyExes.FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return "Không tìm thấy thông tin Ngoại tệ.";
        db.CurrencyExes.Remove(c);
        await db.SaveChangesAsync();
        return null;
    }

    // --- Đơn vị tính theo quy cách (Mst_SpecUnit) ---
    public async Task<List<SpecUnit>> SpecUnitsAsync(string? specCode)
    {
        var query = db.SpecUnits.AsQueryable();
        if (!string.IsNullOrWhiteSpace(specCode)) query = query.Where(u => u.SpecCode == specCode);
        return await query.OrderBy(u => u.SpecCode).ThenBy(u => u.UnitCode).ToListAsync();
    }

    public Task<SpecUnit?> GetSpecUnitAsync(int id) => db.SpecUnits.FirstOrDefaultAsync(u => u.Id == id);

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_SpecUnit_Create / Mst_SpecUnit_Update / Mst_SpecUnit_CheckDB):
    /// quy cách (SpecCode) và ĐVT (UnitCode) bắt buộc và phải tồn tại & đang dùng;
    /// mỗi cặp (SpecCode, UnitCode) chỉ có 1 dòng; ĐVT chuẩn (StandardUnitCode) nếu có phải tồn tại;
    /// hệ số quy đổi Qty không âm; khi sửa phải tồn tại.
    /// Trả về thông báo lỗi hoặc null nếu OK.
    /// </summary>
    public async Task<string?> SaveSpecUnitAsync(SpecUnit u)
    {
        if (string.IsNullOrWhiteSpace(u.SpecCode)) return "Cần chọn quy cách.";
        if (string.IsNullOrWhiteSpace(u.UnitCode)) return "Cần chọn đơn vị tính.";
        u.SpecCode = u.SpecCode.Trim(); u.UnitCode = u.UnitCode.Trim();

        // Mst_SpecUnit_Create: quy cách phải tồn tại & đang dùng (Mst_Spec_CheckDB).
        var spec = await db.Specs.FirstOrDefaultAsync(s => s.Code == u.SpecCode);
        if (spec == null) return $"Quy cách '{u.SpecCode}' không tồn tại.";
        if (!spec.Active) return $"Quy cách '{u.SpecCode}' đã ngừng dùng.";

        // Mst_SpecUnit_Create: ĐVT phải tồn tại & đang dùng (Mst_Unit_CheckDB).
        var unit = await db.Units.FirstOrDefaultAsync(x => x.CodeUser == u.UnitCode || x.Code == u.UnitCode);
        if (unit == null) return $"Đơn vị tính '{u.UnitCode}' không tồn tại.";
        if (!unit.Active) return $"Đơn vị tính '{u.UnitCode}' đã ngừng dùng.";

        // Mst_SpecUnit_Create: ĐVT chuẩn nếu có phải tồn tại & đang dùng.
        if (!string.IsNullOrWhiteSpace(u.StandardUnitCode))
        {
            u.StandardUnitCode = u.StandardUnitCode.Trim();
            var std = await db.Units.FirstOrDefaultAsync(x => x.CodeUser == u.StandardUnitCode || x.Code == u.StandardUnitCode);
            if (std == null) return $"Đơn vị tính chuẩn '{u.StandardUnitCode}' không tồn tại.";
            if (!std.Active) return $"Đơn vị tính chuẩn '{u.StandardUnitCode}' đã ngừng dùng.";
        }

        if (u.Qty < 0) return "Hệ số quy đổi không được âm.";

        // Mst_SpecUnit_CheckDB: mỗi cặp (SpecCode, UnitCode) chỉ có 1 dòng.
        if (u.Id == 0 && await db.SpecUnits.AnyAsync(x => x.SpecCode == u.SpecCode && x.UnitCode == u.UnitCode))
            return $"Đã có đơn vị tính '{u.UnitCode}' cho quy cách '{u.SpecCode}'.";

        SpecUnit target;
        if (u.Id > 0)
        {
            target = await db.SpecUnits.FirstOrDefaultAsync(x => x.Id == u.Id)
                ?? throw new InvalidOperationException("Không tìm thấy thông tin Đơn vị tính theo quy cách.");
            target.StandardUnitCode = u.StandardUnitCode; target.Description = u.Description;
            target.Qty = u.Qty; target.Length = u.Length; target.Width = u.Width; target.Height = u.Height;
            target.Volume = u.Volume; target.Weight = u.Weight; target.Remark = u.Remark;
            target.Active = u.Active; target.UpdatedAt = DateTime.Now;
        }
        else
        {
            target = u;
            db.SpecUnits.Add(target);
        }
        await db.SaveChangesAsync();
        return null;
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_SpecUnit_Delete): đơn vị tính theo quy cách phải tồn tại mới cho xóa.
    /// </summary>
    public async Task<string?> DeleteSpecUnitAsync(int id)
    {
        var u = await db.SpecUnits.FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return "Không tìm thấy thông tin Đơn vị tính theo quy cách.";
        db.SpecUnits.Remove(u);
        await db.SaveChangesAsync();
        return null;
    }

    // --- Trường tùy chỉnh quy cách (Mst_SpecCustomField) ---
    public async Task<List<SpecCustomField>> SpecCustomFieldsAsync(string? q)
    {
        var query = db.SpecCustomFields.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(f => f.Code.Contains(q) || f.Name.Contains(q) || (f.DBPhysicalType ?? "").Contains(q));
        return await query.OrderBy(f => f.Code).ToListAsync();
    }

    public Task<SpecCustomField?> GetSpecCustomFieldAsync(int id) =>
        db.SpecCustomFields.FirstOrDefaultAsync(f => f.Id == id);

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_SpecCustomField_Create / Mst_SpecCustomField_Update /
    /// Mst_SpecCustomField_CheckDB): mã trường tùy chỉnh bắt buộc & không trùng trong tổ chức,
    /// tên trường tùy chỉnh bắt buộc (Mst_SpecCustomField_Update_InvalidSpecCustomFieldName),
    /// khi sửa phải tồn tại (Mst_SpecCustomField_CheckDB_CustomFieldNotFound).
    /// Kiểu dữ liệu vật lý mặc định nvarchar(400) (TConst.BizMix.Default_DBColType).
    /// Trả về thông báo lỗi hoặc null nếu OK.
    /// </summary>
    public async Task<string?> SaveSpecCustomFieldAsync(SpecCustomField f)
    {
        if (string.IsNullOrWhiteSpace(f.Code)) return "Mã Trường tùy chỉnh không hợp lệ.";
        if (string.IsNullOrWhiteSpace(f.Name)) return "Tên Trường tùy chỉnh không hợp lệ.";
        f.Code = f.Code.Trim(); f.Name = f.Name.Trim();

        var dupCode = await db.SpecCustomFields.AnyAsync(x => x.Code == f.Code && x.Id != f.Id);
        if (dupCode) return $"Mã Trường tùy chỉnh '{f.Code}' đã tồn tại.";

        // Mst_SpecCustomField_Create: DBPhysicalType mặc định nvarchar(400).
        if (string.IsNullOrWhiteSpace(f.DBPhysicalType)) f.DBPhysicalType = "nvarchar(400)";

        SpecCustomField target;
        if (f.Id > 0)
        {
            target = await db.SpecCustomFields.FirstOrDefaultAsync(x => x.Id == f.Id)
                ?? throw new InvalidOperationException("Không tìm thấy thông tin Trường tùy chỉnh.");
            target.Name = f.Name; target.NetworkId = f.NetworkId; target.DBPhysicalType = f.DBPhysicalType;
            target.Remark = f.Remark; target.Active = f.Active; target.UpdatedAt = DateTime.Now;
        }
        else
        {
            target = f;
            db.SpecCustomFields.Add(target);
        }
        await db.SaveChangesAsync();
        return null;
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_SpecCustomField_CheckDB): trường tùy chỉnh phải tồn tại mới cho xóa.
    /// </summary>
    public async Task<string?> DeleteSpecCustomFieldAsync(int id)
    {
        var f = await db.SpecCustomFields.FirstOrDefaultAsync(x => x.Id == id);
        if (f == null) return "Không tìm thấy thông tin Trường tùy chỉnh.";
        db.SpecCustomFields.Remove(f);
        await db.SaveChangesAsync();
        return null;
    }

    // --- Loại quy cách (Mst_SpecType1 / Mst_SpecType2) ---
    public async Task<List<SpecType>> SpecTypesAsync(int? kind, string? q)
    {
        var query = db.SpecTypes.AsQueryable();
        if (kind.HasValue) query = query.Where(t => t.Kind == kind.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.Code.Contains(q) || t.Name.Contains(q));
        return await query.OrderBy(t => t.Kind).ThenBy(t => t.Code).ToListAsync();
    }

    public Task<SpecType?> GetSpecTypeAsync(int id) => db.SpecTypes.FirstOrDefaultAsync(t => t.Id == id);

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_SpecType1_Create / Mst_SpecType1_Update / Mst_SpecType1_CheckDB,
    /// áp dụng chung cho SpecType2): mã loại quy cách bắt buộc (Mst_SpecType1_Create_InvalidSpecType1),
    /// mã không trùng trong tổ chức theo từng Kind (Mst_SpecType1_CheckDB_SpecType1Exist),
    /// tên loại bắt buộc (Mst_SpecType1_Create_InvalidSpecType1Name / _Update_InvalidSpecType1Name),
    /// khi sửa phải tồn tại. Trả về thông báo lỗi hoặc null nếu OK.
    /// </summary>
    public async Task<string?> SaveSpecTypeAsync(SpecType t)
    {
        if (t.Kind != 1 && t.Kind != 2) return "Loại quy cách không hợp lệ.";
        if (string.IsNullOrWhiteSpace(t.Code)) return "Mã Loại quy cách không hợp lệ.";
        t.Code = t.Code.Trim();
        t.Name = (t.Name ?? "").Trim();

        // Mst_SpecType1_CheckDB: mã không trùng trong tổ chức (theo từng Kind).
        var dupCode = await db.SpecTypes.AnyAsync(x => x.Kind == t.Kind && x.Code == t.Code && x.Id != t.Id);
        if (dupCode) return $"Mã Loại quy cách '{t.Code}' đã tồn tại.";

        // Mst_SpecType1_Create_InvalidSpecType1Name / _Update_InvalidSpecType1Name.
        if (string.IsNullOrWhiteSpace(t.Name)) return "Tên Loại quy cách không hợp lệ.";

        SpecType target;
        if (t.Id > 0)
        {
            target = await db.SpecTypes.FirstOrDefaultAsync(x => x.Id == t.Id)
                ?? throw new InvalidOperationException("Không tìm thấy thông tin Loại quy cách.");
            target.Name = t.Name; target.NetworkId = t.NetworkId;
            target.Remark = t.Remark; target.Active = t.Active; target.UpdatedAt = DateTime.Now;
        }
        else
        {
            target = t;
            db.SpecTypes.Add(target);
        }
        await db.SaveChangesAsync();
        return null;
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_SpecType1_Delete / Mst_SpecType1_CheckDB):
    /// loại quy cách phải tồn tại mới cho xóa (Mst_SpecType1_CheckDB_SpecType1NotFound).
    /// </summary>
    public async Task<string?> DeleteSpecTypeAsync(int id)
    {
        var t = await db.SpecTypes.FirstOrDefaultAsync(x => x.Id == id);
        if (t == null) return "Không tìm thấy thông tin Loại quy cách.";
        db.SpecTypes.Remove(t);
        await db.SaveChangesAsync();
        return null;
    }
}
