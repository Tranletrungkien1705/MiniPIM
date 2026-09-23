using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniPIM.Data;
using MiniPIM.Models;
using MiniPIM.Services;

namespace MiniPIM.Controllers;

public class HomeController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index() { ViewBag.Dash = await svc.DashboardAsync(); return View(); }
}

public class ProductController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q, int? groupId)
    {
        ViewBag.Q = q; ViewBag.GroupId = groupId; ViewBag.Groups = await svc.GroupsAsync();
        return View(await svc.ProductsAsync(q, groupId));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        ViewBag.Groups = await svc.GroupsAsync();
        ViewBag.SsccTypes = await svc.SsccTypesAsync(null);
        var p = id.HasValue ? await svc.GetAsync(id.Value) : new Product();
        if (p == null) return NotFound();
        return View(p);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string? codeUser, string name, int? groupId, string uom, string? barcode,
        decimal costPrice, decimal salePrice, string? description, ProductStatus status,
        ProductLevel level, decimal valConvert, decimal qtyMinSt, decimal qtyMaxSt, string? vatRateCode,
        bool flagSerial, bool flagLot, string? origin, string? quyCach, string? ssccTypeCode, string? gtin,
        string[]? attrName, string[]? attrValue, string[]? bomCode, string[]? bomName, decimal[]? bomQty, string[]? bomUom)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Cần tên sản phẩm."; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        var p = new Product { Id = id, Code = code ?? "", CodeUser = string.IsNullOrWhiteSpace(codeUser) ? null : codeUser.Trim(),
            Name = name.Trim(), GroupId = groupId, Uom = string.IsNullOrWhiteSpace(uom) ? "cái" : uom,
            Barcode = barcode, CostPrice = costPrice, SalePrice = salePrice, Description = description, Status = status,
            Level = level, ValConvert = valConvert <= 0 ? 1 : valConvert, QtyMinSt = qtyMinSt, QtyMaxSt = qtyMaxSt,
            VatRateCode = vatRateCode, FlagSerial = flagSerial, FlagLot = flagLot, Origin = origin, QuyCach = quyCach,
            SsccTypeCode = ssccTypeCode, Gtin = gtin };
        // Mst_Product_CheckProductCodeUser: Mã Hàng hóa người dùng phải duy nhất trong tổ chức.
        var codeUserErr = await svc.ValidateProductCodeUserAsync(p);
        if (codeUserErr != null) { TempData["Error"] = codeUserErr; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        var attrs = new List<ProductAttribute>();
        for (int i = 0; attrName != null && i < attrName.Length; i++)
            attrs.Add(new ProductAttribute { Name = attrName[i], Value = i < (attrValue?.Length ?? 0) ? attrValue![i] : "" });
        var bom = new List<BomLine>();
        for (int i = 0; bomName != null && i < bomName.Length; i++)
            bom.Add(new BomLine { ComponentCode = i < (bomCode?.Length ?? 0) ? bomCode![i] : "", ComponentName = bomName[i],
                Quantity = i < (bomQty?.Length ?? 0) ? bomQty![i] : 1, Uom = i < (bomUom?.Length ?? 0) ? bomUom![i] : "cái" });
        // Nghiệp vụ ProductCenter (Mst_Product_CreateX / Mst_Product_UpdateMasterX):
        // kiểm tra tham chiếu danh mục (Loại hàng hóa / Thuế suất / ĐVT) + quy tắc Combo + BOM serial/lô.
        var err = await svc.ValidateProductMasterAsync(p, bom);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        var newId = await svc.SaveProductAsync(p, attrs, bom);
        TempData["Success"] = "Đã lưu sản phẩm.";
        return RedirectToAction(nameof(Edit), new { id = newId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var err = await svc.DeleteProductAsync(id);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id }); }
        TempData["Success"] = "Đã xóa sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkUsed(int id, DateTime? dtimeUsed)
    {
        var err = await svc.MarkProductUsedAsync(id, dtimeUsed);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã đánh dấu hàng hóa đã sử dụng.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    /// <summary>
    /// Nghiệp vụ ProductCenter (Mst_Product_CheckProductCodeUser): tra hàng hóa theo
    /// Mã Hàng hóa người dùng (ProductCodeUser).
    /// </summary>
    public async Task<IActionResult> ByCodeUser(string codeUser)
    {
        if (string.IsNullOrWhiteSpace(codeUser)) { TempData["Error"] = "Cần nhập Mã Hàng hóa người dùng."; return RedirectToAction(nameof(Index)); }
        var p = await svc.GetByCodeUserAsync(codeUser.Trim());
        if (p == null) { TempData["Error"] = $"Không tìm thấy Mã Hàng hóa '{codeUser}'."; return RedirectToAction(nameof(Index)); }
        return RedirectToAction(nameof(Edit), new { id = p.Id });
    }
}

public class GroupController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Groups = await svc.GroupsAsync();
        return View(await svc.GroupsAsync());
    }

    public async Task<IActionResult> Edit(int? id)
    {
        ViewBag.Groups = await svc.GroupsAsync();
        ViewBag.Brands = await svc.BrandsAsync(null);
        var g = id.HasValue ? await svc.GetGroupAsync(id.Value) : new ProductGroup();
        if (g == null) return NotFound();
        return View(g);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string name, string? parentCode, string? brandCode, bool flagFG, bool active)
    {
        var g = new ProductGroup { Id = id, Code = code ?? "", Name = name ?? "", ParentCode = parentCode, BrandCode = brandCode, FlagFG = flagFG, Active = active };
        var err = await svc.SaveGroupAsync(g);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu nhóm hàng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? code)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Cần tên nhóm."; return RedirectToAction(nameof(Index)); }
        await svc.CreateGroupAsync(new ProductGroup { Name = name.Trim(), Code = code ?? "" });
        TempData["Success"] = "Đã tạo nhóm.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var err = await svc.DeleteGroupAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa nhóm hàng.";
        return RedirectToAction(nameof(Index));
    }
}

public class AttributeController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.AttributeDefsAsync(q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var a = id.HasValue ? await svc.GetAttributeDefAsync(id.Value) : new AttributeDef();
        if (a == null) return NotFound();
        return View(a);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string name, string? networkId, bool active)
    {
        var a = new AttributeDef { Id = id, Code = code ?? "", Name = name ?? "", NetworkId = networkId, Active = active };
        var err = await svc.SaveAttributeDefAsync(a);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu thuộc tính.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var err = await svc.DeleteAttributeDefAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa thuộc tính.";
        return RedirectToAction(nameof(Index));
    }
}

public class UnitController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.UnitsAsync(q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var u = id.HasValue ? await svc.GetUnitAsync(id.Value) : new Unit();
        if (u == null) return NotFound();
        return View(u);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string codeUser, string name, string? remark, bool active)
    {
        var u = new Unit { Id = id, Code = code ?? "", CodeUser = codeUser ?? "", Name = name ?? "", Remark = remark, Active = active };
        var err = await svc.SaveUnitAsync(u);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu đơn vị tính.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var err = await svc.DeleteUnitAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa đơn vị tính.";
        return RedirectToAction(nameof(Index));
    }
}

public class VatRateController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.VatRatesAsync(q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var v = id.HasValue ? await svc.GetVatRateAsync(id.Value) : new VatRate();
        if (v == null) return NotFound();
        return View(v);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, decimal rate, string name, bool active)
    {
        var v = new VatRate { Id = id, Code = code ?? "", Rate = rate, Name = name ?? "", Active = active };
        var err = await svc.SaveVatRateAsync(v);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu thuế suất.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var err = await svc.DeleteVatRateAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa thuế suất.";
        return RedirectToAction(nameof(Index));
    }
}

public class SpecController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.SpecsAsync(q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var s = id.HasValue ? await svc.GetSpecAsync(id.Value) : new Spec();
        if (s == null) return NotFound();
        return View(s);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string name, string? description, string? modelCode,
        string? specType1, string? specType2, string? color, bool flagHasSerial, bool flagHasLot,
        string? defaultUnitCode, string? standardUnitCode, string? remark, bool active)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Cần tên quy cách."; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        var s = new Spec { Id = id, Code = code ?? "", Name = name.Trim(), Description = description, ModelCode = modelCode,
            SpecType1 = specType1, SpecType2 = specType2, Color = color, FlagHasSerial = flagHasSerial, FlagHasLot = flagHasLot,
            DefaultUnitCode = defaultUnitCode, StandardUnitCode = standardUnitCode, Remark = remark, Active = active };
        var newId = await svc.SaveSpecAsync(s);
        TempData["Success"] = "Đã lưu quy cách.";
        return RedirectToAction(nameof(Edit), new { id = newId });
    }
}

public class SpecPriceController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? specCode)
    {
        ViewBag.SpecCode = specCode;
        ViewBag.Specs = await svc.SpecsAsync(null);
        return View(await svc.SpecPricesAsync(specCode));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string specCode, string unitCode, decimal buyPrice, decimal sellPrice,
        string? currencyCode, string? vatRateCode, decimal discountVnd, DateTime effectDTimeStart, DateTime effectDTimeEnd,
        string? remark, bool active)
    {
        var p = new SpecPrice { Id = id, SpecCode = specCode ?? "", UnitCode = unitCode ?? "", BuyPrice = buyPrice, SellPrice = sellPrice,
            CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "VND" : currencyCode, VatRateCode = vatRateCode,
            DiscountVnd = discountVnd, EffectDTimeStart = effectDTimeStart, EffectDTimeEnd = effectDTimeEnd,
            Remark = remark, Active = active };
        var err = await svc.SaveSpecPriceAsync(p);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Index), new { specCode }); }
        TempData["Success"] = "Đã lưu giá theo quy cách.";
        return RedirectToAction(nameof(Index), new { specCode });
    }
}

public class BrandController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.BrandsAsync(q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var b = id.HasValue ? await svc.GetBrandAsync(id.Value) : new Brand();
        if (b == null) return NotFound();
        return View(b);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string name, string? networkBrandCode, string? remark, bool active)
    {
        var b = new Brand { Id = id, Code = code ?? "", Name = name ?? "", NetworkBrandCode = networkBrandCode, Remark = remark, Active = active };
        var err = await svc.SaveBrandAsync(b);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu nhãn hiệu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var err = await svc.DeleteBrandAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa nhãn hiệu.";
        return RedirectToAction(nameof(Index));
    }
}

public class ModelController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.ModelsAsync(q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        ViewBag.Brands = await svc.BrandsAsync(null);
        var m = id.HasValue ? await svc.GetModelAsync(id.Value) : new Model();
        if (m == null) return NotFound();
        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string name, string? orgModelCode, string? brandCode,
        string? networkModelCode, string? remark, bool active)
    {
        var m = new Model { Id = id, Code = code ?? "", Name = name ?? "", OrgModelCode = orgModelCode,
            BrandCode = brandCode, NetworkModelCode = networkModelCode, Remark = remark, Active = active };
        var err = await svc.SaveModelAsync(m);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu model.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var err = await svc.DeleteModelAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa model.";
        return RedirectToAction(nameof(Index));
    }
}

public class ProductTypeController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.ProductTypesAsync(q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var t = id.HasValue ? await svc.GetProductTypeAsync(id.Value) : new ProductType();
        if (t == null) return NotFound();
        return View(t);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string name, string? remark, bool active)
    {
        var t = new ProductType { Id = id, Code = code ?? "", Name = name ?? "", Remark = remark, Active = active };
        var err = await svc.SaveProductTypeAsync(t);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu loại hàng hóa.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var err = await svc.DeleteProductTypeAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa loại hàng hóa.";
        return RedirectToAction(nameof(Index));
    }
}

public class CurrencyExController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.CurrencyExesAsync(q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var c = id.HasValue ? await svc.GetCurrencyExAsync(id.Value) : new CurrencyEx();
        if (c == null) return NotFound();
        return View(c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string name, string? baseCurrencyCode,
        decimal buyRate, decimal sellRate, decimal interEx, string? remark, bool active)
    {
        var c = new CurrencyEx { Id = id, Code = code ?? "", Name = name ?? "", BaseCurrencyCode = baseCurrencyCode,
            BuyRate = buyRate, SellRate = sellRate, InterEx = interEx, Remark = remark, Active = active };
        var err = await svc.SaveCurrencyExAsync(c);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu tỷ giá ngoại tệ.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var err = await svc.DeleteCurrencyExAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa tỷ giá ngoại tệ.";
        return RedirectToAction(nameof(Index));
    }
}

public class SpecUnitController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? specCode)
    {
        ViewBag.SpecCode = specCode;
        ViewBag.Specs = await svc.SpecsAsync(null);
        ViewBag.Units = await svc.UnitsAsync(null);
        return View(await svc.SpecUnitsAsync(specCode));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        ViewBag.Specs = await svc.SpecsAsync(null);
        ViewBag.Units = await svc.UnitsAsync(null);
        var u = id.HasValue ? await svc.GetSpecUnitAsync(id.Value) : new SpecUnit();
        if (u == null) return NotFound();
        return View(u);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string specCode, string unitCode, string? standardUnitCode,
        string? description, decimal qty, decimal? length, decimal? width, decimal? height, decimal? volume,
        decimal? weight, string? remark, bool active)
    {
        var u = new SpecUnit { Id = id, SpecCode = specCode ?? "", UnitCode = unitCode ?? "",
            StandardUnitCode = standardUnitCode, Description = description, Qty = qty <= 0 ? 1 : qty,
            Length = length, Width = width, Height = height, Volume = volume, Weight = weight,
            Remark = remark, Active = active };
        var err = await svc.SaveSpecUnitAsync(u);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu đơn vị tính theo quy cách.";
        return RedirectToAction(nameof(Index), new { specCode });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? specCode)
    {
        var err = await svc.DeleteSpecUnitAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa đơn vị tính theo quy cách.";
        return RedirectToAction(nameof(Index), new { specCode });
    }
}

public class SpecCustomFieldController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.SpecCustomFieldsAsync(q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var f = id.HasValue ? await svc.GetSpecCustomFieldAsync(id.Value) : new SpecCustomField();
        if (f == null) return NotFound();
        return View(f);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string name, string? networkId, string? dbPhysicalType,
        string? remark, bool active)
    {
        var f = new SpecCustomField { Id = id, Code = code ?? "", Name = name ?? "", NetworkId = networkId,
            DBPhysicalType = dbPhysicalType, Remark = remark, Active = active };
        var err = await svc.SaveSpecCustomFieldAsync(f);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu trường tùy chỉnh quy cách.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var err = await svc.DeleteSpecCustomFieldAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa trường tùy chỉnh quy cách.";
        return RedirectToAction(nameof(Index));
    }
}

public class SpecTypeController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(int? kind, string? q)
    {
        ViewBag.Kind = kind; ViewBag.Q = q;
        return View(await svc.SpecTypesAsync(kind, q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var t = id.HasValue ? await svc.GetSpecTypeAsync(id.Value) : new SpecType();
        if (t == null) return NotFound();
        return View(t);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, int kind, string code, string name, string? networkId, string? remark, bool active)
    {
        var t = new SpecType { Id = id, Kind = kind, Code = code ?? "", Name = name ?? "", NetworkId = networkId, Remark = remark, Active = active };
        var err = await svc.SaveSpecTypeAsync(t);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu loại quy cách.";
        return RedirectToAction(nameof(Index), new { kind });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? kind)
    {
        var err = await svc.DeleteSpecTypeAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa loại quy cách.";
        return RedirectToAction(nameof(Index), new { kind });
    }
}

public class SsccTypeController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.SsccTypesAsync(q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var s = id.HasValue ? await svc.GetSsccTypeAsync(id.Value) : new SsccType();
        if (s == null) return NotFound();
        return View(s);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string name, string? networkId, bool active)
    {
        var s = new SsccType { Id = id, Code = code ?? "", Name = name ?? "", NetworkId = networkId, Active = active };
        var err = await svc.SaveSsccTypeAsync(s);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu loại SSCC.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var err = await svc.DeleteSsccTypeAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa loại SSCC.";
        return RedirectToAction(nameof(Index));
    }
}

public class ProductImageController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? productCode)
    {
        ViewBag.ProductCode = productCode;
        ViewBag.Products = await svc.ProductsAsync(null, null);
        return View(await svc.ProductImagesAsync(productCode));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        ViewBag.Products = await svc.ProductsAsync(null, null);
        var img = id.HasValue ? await svc.GetProductImageAsync(id.Value) : new ProductImage();
        if (img == null) return NotFound();
        return View(img);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string productCode, int idx, string? networkId, string? imagePath,
        string? imageName, string? imageDesc, bool flagPrimaryImage, bool active)
    {
        var img = new ProductImage { Id = id, ProductCode = productCode ?? "", Idx = idx, NetworkId = networkId,
            ImagePath = imagePath, ImageName = imageName, ImageDesc = imageDesc,
            FlagPrimaryImage = flagPrimaryImage, Active = active };
        var err = await svc.SaveProductImageAsync(img);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu ảnh hàng hóa.";
        return RedirectToAction(nameof(Index), new { productCode });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? productCode)
    {
        var err = await svc.DeleteProductImageAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa ảnh hàng hóa.";
        return RedirectToAction(nameof(Index), new { productCode });
    }
}

public class ProductFileController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? productCode)
    {
        ViewBag.ProductCode = productCode;
        ViewBag.Products = await svc.ProductsAsync(null, null);
        return View(await svc.ProductFilesAsync(productCode));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        ViewBag.Products = await svc.ProductsAsync(null, null);
        var f = id.HasValue ? await svc.GetProductFileAsync(id.Value) : new ProductFile();
        if (f == null) return NotFound();
        return View(f);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string productCode, int idx, string? networkId, string? filePath,
        string? fileName, string? fileDesc, bool active)
    {
        var f = new ProductFile { Id = id, ProductCode = productCode ?? "", Idx = idx, NetworkId = networkId,
            FilePath = filePath, FileName = fileName, FileDesc = fileDesc, Active = active };
        var err = await svc.SaveProductFileAsync(f);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu file đính kèm hàng hóa.";
        return RedirectToAction(nameof(Index), new { productCode });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? productCode)
    {
        var err = await svc.DeleteProductFileAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa file đính kèm hàng hóa.";
        return RedirectToAction(nameof(Index), new { productCode });
    }
}

public class ProductCustomFieldController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.ProductCustomFieldsAsync(q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var f = id.HasValue ? await svc.GetProductCustomFieldAsync(id.Value) : new ProductCustomField();
        if (f == null) return NotFound();
        return View(f);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string name, string? networkId, string? dbPhysicalType, bool active)
    {
        var f = new ProductCustomField { Id = id, Code = code ?? "", Name = name ?? "", NetworkId = networkId,
            DBPhysicalType = dbPhysicalType, Active = active };
        var err = await svc.SaveProductCustomFieldAsync(f);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu trường động của Hàng hóa.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var err = await svc.DeleteProductCustomFieldAsync(id);
        if (err != null) TempData["Error"] = err; else TempData["Success"] = "Đã xóa trường động của Hàng hóa.";
        return RedirectToAction(nameof(Index));
    }
}

public class MasterCheckController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await svc.AuditMasterAsync());
    }
}

/// <summary>
/// Phân cấp hàng hóa (Mst_Product: Root / Base / L2) — nguồn 2019.4.ProductCenter.
/// Một hàng gốc (Root) chứa nhiều hàng cơ sở (Base); mỗi hàng cơ sở lại chứa nhiều
/// hàng cấp 2 (L2). Điều hướng theo ProductCodeRoot/ProductCodeBase.
/// </summary>
public class ProductHierarchyController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await svc.RootsAsync(q));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        ViewBag.Groups = await svc.GroupsAsync();
        ViewBag.Roots = await svc.RootsAsync(null);
        var p = id.HasValue ? await svc.GetAsync(id.Value) : new Product { Level = ProductLevel.Root };
        if (p == null) return NotFound();
        return View(p);
    }

    /// <summary>Chi tiết một hàng gốc: danh sách hàng cơ sở và hàng cấp 2 (Mst_Product_Get_Children / Get_Level2).</summary>
    public async Task<IActionResult> Tree(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return RedirectToAction(nameof(Index));
        ViewBag.Root = await svc.GetByCodeAsync(code);
        if (ViewBag.Root == null) return NotFound();
        ViewBag.RootCode = code;
        return View(await svc.ChildrenAsync(code));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string name, int? groupId, string uom,
        string? barcode, decimal costPrice, decimal salePrice, string? description,
        ProductStatus status, string? productCodeRoot, string? productCodeBase)
    {
        var p = new Product { Id = id, Code = code ?? "", Name = name ?? "", GroupId = groupId,
            Uom = string.IsNullOrWhiteSpace(uom) ? "cái" : uom, Barcode = barcode, CostPrice = costPrice,
            SalePrice = salePrice, Description = description, Status = status,
            ProductCodeRoot = productCodeRoot, ProductCodeBase = productCodeBase };
        var err = await svc.SaveProductHierarchyAsync(p);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        TempData["Success"] = "Đã lưu hàng hóa phân cấp.";
        return RedirectToAction(nameof(Index));
    }
}

public class OrgController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var orgs = await db.Orgs.IgnoreQueryFilters().OrderBy(o => o.CreatedAt).ToListAsync();
        Request.Cookies.TryGetValue(TenantContext.CookieName, out var curKey);
        ViewBag.CurrentKey = curKey ?? TenantContext.DefaultApiKey;
        return View(orgs);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Cần tên tổ chức."; return RedirectToAction(nameof(Index)); }
        var org = new Org { Name = name.Trim(), ApiKey = "pim_" + Guid.NewGuid().ToString("N") };
        db.Orgs.Add(org); await db.SaveChangesAsync();
        SetCookies(org.ApiKey, org.Name);
        TempData["Success"] = $"Đã tạo & chuyển sang \"{org.Name}\".";
        return RedirectToAction("Index", "Home");
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Switch(string apiKey)
    {
        var org = await db.Orgs.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.ApiKey == apiKey);
        if (org == null) { TempData["Error"] = "Không tìm thấy."; return RedirectToAction(nameof(Index)); }
        SetCookies(org.ApiKey, org.Name);
        return RedirectToAction("Index", "Home");
    }
    public IActionResult Reset()
    {
        Response.Cookies.Delete(TenantContext.CookieName); Response.Cookies.Delete("org_name");
        return RedirectToAction("Index", "Home");
    }
    private void SetCookies(string k, string n)
    {
        var o = new CookieOptions { IsEssential = true, Expires = DateTimeOffset.UtcNow.AddDays(30) };
        Response.Cookies.Append(TenantContext.CookieName, k, o); Response.Cookies.Append("org_name", n, o);
    }
}
