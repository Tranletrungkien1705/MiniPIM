using Microsoft.AspNetCore.Mvc;
using MiniPIM.Data;
using MiniPIM.Models;
using MiniPIM.Services;

namespace MiniPIM.Controllers;

/// <summary>
/// API JSON cho SPA React. DTO phẳng. Dashboard cache Redis 30s theo tenant (X-Cache).
/// Sản phẩm có thuộc tính động + BOM (định mức). Request DTO = class get/set.
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class ApiV1Controller(IProductService svc, ICache cache, ITenantContext tenant) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var key = $"pim:dash:{tenant.OrgId}";
        var hit = await cache.GetAsync<DashDto>(key);
        if (hit != null) { Response.Headers["X-Cache"] = "HIT"; return Ok(hit); }
        var d = await svc.DashboardAsync();
        var dto = new DashDto(d.Products, d.Active, d.Groups, d.WithBom,
            d.ByGroup.Select(g => new ByGroupDto(g.Group, g.Count)).ToList());
        await cache.SetAsync(key, dto, TimeSpan.FromSeconds(30));
        Response.Headers["X-Cache"] = "MISS";
        return Ok(dto);
    }

    [HttpGet("groups")]
    public async Task<IActionResult> Groups()
        => Ok((await svc.GroupsAsync()).Select(g => new { g.Id, g.Code, g.Name }));

    [HttpPost("groups")]
    public async Task<IActionResult> CreateGroup([FromBody] GroupReq r)
    {
        if (string.IsNullOrWhiteSpace(r.Name)) return BadRequest(new { error = "Cần tên nhóm." });
        var id = await svc.CreateGroupAsync(new ProductGroup { Name = r.Name.Trim(), Code = r.Code ?? "" });
        return Ok(new { id });
    }

    [HttpGet("products")]
    public async Task<IActionResult> Products([FromQuery] string? q, [FromQuery] int? groupId)
        => Ok((await svc.ProductsAsync(q, groupId)).Select(ToListDto));

    [HttpGet("products/{id:int}")]
    public async Task<IActionResult> Product(int id)
    {
        var p = await svc.GetAsync(id);
        return p == null ? NotFound(new { error = "Không tìm thấy sản phẩm." }) : Ok(ToDetailDto(p));
    }

    [HttpPost("products")]
    public async Task<IActionResult> Save([FromBody] ProductReq r)
    {
        if (string.IsNullOrWhiteSpace(r.Name)) return BadRequest(new { error = "Cần tên sản phẩm." });
        var p = new Product
        {
            Id = r.Id, Code = r.Code ?? "", Name = r.Name.Trim(), GroupId = r.GroupId, Uom = string.IsNullOrWhiteSpace(r.Uom) ? "cái" : r.Uom!,
            Barcode = r.Barcode, CostPrice = r.CostPrice, SalePrice = r.SalePrice, ImageUrl = r.ImageUrl,
            Description = r.Description, Status = (ProductStatus)r.Status
        };
        var attrs = (r.Attributes ?? new()).Select(a => new ProductAttribute { Name = a.Name, Value = a.Value }).ToList();
        var bom = (r.Bom ?? new()).Select(b => new BomLine { ComponentCode = b.ComponentCode ?? "", ComponentName = b.ComponentName, Quantity = b.Quantity, Uom = string.IsNullOrWhiteSpace(b.Uom) ? "cái" : b.Uom! }).ToList();
        var id = await svc.SaveProductAsync(p, attrs, bom);
        return Ok(new { id });
    }

    // Catalog công khai theo mã (cấp cho WMS/DMS/Stamp) — chỉ SP Active.
    [HttpGet("catalog/{code}")]
    public async Task<IActionResult> Catalog(string code)
    {
        var p = await svc.GetByCodeAsync(code);
        if (p == null || p.Status != ProductStatus.Active) return NotFound(new { error = "Không tìm thấy sản phẩm Active." });
        return Ok(new { p.Code, p.Name, p.Uom, p.Barcode, p.SalePrice, group = p.Group?.Name, attributes = p.Attributes.Select(a => new { a.Name, a.Value }) });
    }

    private static object ToListDto(Product p) => new
    {
        p.Id, p.Code, p.Name, group = p.Group?.Name, p.Uom, p.Barcode, p.CostPrice, p.SalePrice,
        status = (int)p.Status, statusText = p.Status == ProductStatus.Active ? "Đang bán" : "Ngừng", p.UpdatedAt
    };
    private static object ToDetailDto(Product p) => new
    {
        p.Id, p.Code, p.Name, p.GroupId, group = p.Group?.Name, p.Uom, p.Barcode, p.CostPrice, p.SalePrice,
        p.ImageUrl, p.Description, status = (int)p.Status,
        attributes = p.Attributes.Select(a => new { a.Name, a.Value }),
        bom = p.Bom.Select(b => new { b.ComponentCode, b.ComponentName, b.Quantity, b.Uom })
    };
}

public record DashDto(int Products, int Active, int Groups, int WithBom, List<ByGroupDto> ByGroup);
public record ByGroupDto(string Group, int Count);

public class GroupReq { public string Name { get; set; } = ""; public string? Code { get; set; } }
public class AttrReq { public string Name { get; set; } = ""; public string? Value { get; set; } }
public class BomReq { public string? ComponentCode { get; set; } public string ComponentName { get; set; } = ""; public decimal Quantity { get; set; } = 1; public string? Uom { get; set; } }
public class ProductReq
{
    public int Id { get; set; } public string? Code { get; set; } public string Name { get; set; } = "";
    public int? GroupId { get; set; } public string? Uom { get; set; } public string? Barcode { get; set; }
    public decimal CostPrice { get; set; } public decimal SalePrice { get; set; }
    public string? ImageUrl { get; set; } public string? Description { get; set; } public int Status { get; set; }
    public List<AttrReq>? Attributes { get; set; } public List<BomReq>? Bom { get; set; }
}
