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
        var p = id.HasValue ? await svc.GetAsync(id.Value) : new Product();
        if (p == null) return NotFound();
        return View(p);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string code, string name, int? groupId, string uom, string? barcode,
        decimal costPrice, decimal salePrice, string? description, ProductStatus status,
        ProductLevel level, decimal valConvert, decimal qtyMinSt, decimal qtyMaxSt, string? vatRateCode,
        bool flagSerial, bool flagLot, string? origin, string? quyCach,
        string[]? attrName, string[]? attrValue, string[]? bomCode, string[]? bomName, decimal[]? bomQty, string[]? bomUom)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Cần tên sản phẩm."; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        var p = new Product { Id = id, Code = code ?? "", Name = name.Trim(), GroupId = groupId, Uom = string.IsNullOrWhiteSpace(uom) ? "cái" : uom,
            Barcode = barcode, CostPrice = costPrice, SalePrice = salePrice, Description = description, Status = status,
            Level = level, ValConvert = valConvert <= 0 ? 1 : valConvert, QtyMinSt = qtyMinSt, QtyMaxSt = qtyMaxSt,
            VatRateCode = vatRateCode, FlagSerial = flagSerial, FlagLot = flagLot, Origin = origin, QuyCach = quyCach };
        var attrs = new List<ProductAttribute>();
        for (int i = 0; attrName != null && i < attrName.Length; i++)
            attrs.Add(new ProductAttribute { Name = attrName[i], Value = i < (attrValue?.Length ?? 0) ? attrValue![i] : "" });
        var bom = new List<BomLine>();
        for (int i = 0; bomName != null && i < bomName.Length; i++)
            bom.Add(new BomLine { ComponentCode = i < (bomCode?.Length ?? 0) ? bomCode![i] : "", ComponentName = bomName[i],
                Quantity = i < (bomQty?.Length ?? 0) ? bomQty![i] : 1, Uom = i < (bomUom?.Length ?? 0) ? bomUom![i] : "cái" });
        // Nghiệp vụ ProductCenter: thành phần BOM không được quản lý serial/lô.
        var err = await svc.ValidateBomAsync(bom);
        if (err != null) { TempData["Error"] = err; return RedirectToAction(nameof(Edit), new { id = id > 0 ? id : (int?)null }); }
        var newId = await svc.SaveProductAsync(p, attrs, bom);
        TempData["Success"] = "Đã lưu sản phẩm.";
        return RedirectToAction(nameof(Edit), new { id = newId });
    }
}

public class GroupController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.GroupsAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? code)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Cần tên nhóm."; return RedirectToAction(nameof(Index)); }
        await svc.CreateGroupAsync(new ProductGroup { Name = name.Trim(), Code = code ?? "" });
        TempData["Success"] = "Đã tạo nhóm.";
        return RedirectToAction(nameof(Index));
    }
}

public class AttributeController(IProductService svc) : Controller
{
    public async Task<IActionResult> Index() => View(await svc.AttributeDefsAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? code)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Cần tên thuộc tính."; return RedirectToAction(nameof(Index)); }
        await svc.CreateAttributeDefAsync(new AttributeDef { Name = name.Trim(), Code = code ?? "" });
        TempData["Success"] = "Đã tạo thuộc tính.";
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
