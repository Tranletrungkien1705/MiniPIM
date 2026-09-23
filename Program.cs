using Microsoft.EntityFrameworkCore;
using MiniPIM.Data;
using MiniPIM.Models;
using MiniPIM.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT") ?? "8080"}");

var conn = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=minipim.db";
builder.Services.AddDbContext<AppDbContext>(o =>
{
    if (DbUtil.IsPostgres(conn)) o.UseNpgsql(DbUtil.ToNpgsql(conn));
    else o.UseSqlite(conn);
});
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
    await Seeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());

app.Use(async (ctx, next) =>
{
    var key = ctx.Request.Headers["X-Api-Key"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(key)) ctx.Request.Cookies.TryGetValue(TenantContext.CookieName, out key);
    if (!string.IsNullOrWhiteSpace(key))
    {
        using var lookup = app.Services.CreateScope();
        var ldb = lookup.ServiceProvider.GetRequiredService<AppDbContext>();
        var org = await ldb.Orgs.FirstOrDefaultAsync(o => o.ApiKey == key);
        if (org != null) ctx.RequestServices.GetRequiredService<ITenantContext>().OrgId = org.Id;
    }
    await next();
});

app.UseStaticFiles();
app.MapGet("/healthz", () => "ok");

// Catalog API — cấp danh mục sản phẩm chuẩn cho MiniDMS/MiniWMS/MiniStamp.
app.MapGet("/api/products", async (string? q, IProductService svc) =>
{
    var list = await svc.ProductsAsync(q, null);
    return Results.Ok(list.Where(p => p.Status == ProductStatus.Active).Select(p => new
    {
        p.Code, p.Name, group = p.Group?.Name, p.Uom, p.Barcode, p.CostPrice, p.SalePrice
    }));
});
app.MapGet("/api/products/{code}", async (string code, IProductService svc) =>
{
    var p = await svc.GetByCodeAsync(code);
    return p is null ? Results.NotFound() : Results.Ok(new
    {
        p.Code, p.Name, group = p.Group?.Name, p.Uom, p.Barcode, p.CostPrice, p.SalePrice, p.Description,
        level = p.Level.ToString(), p.ValConvert, p.QtyMinSt, p.QtyMaxSt, p.VatRateCode, p.FlagSerial, p.FlagLot, p.Origin, p.QuyCach, p.ProductTypeCode, p.SsccTypeCode, p.Gtin,
        dtimeUsed = p.DTimeUsed?.ToString("yyyy-MM-dd"),
        attributes = p.Attributes.Select(a => new { a.Name, a.Value }),
        bom = p.Bom.Select(x => new { x.ComponentCode, x.ComponentName, x.Quantity, x.Uom })
    });
});

// Quy cách & bảng giá theo quy cách (Mst_Spec / Mst_SpecPrice).
app.MapGet("/api/specs", async (string? q, IProductService svc) =>
{
    var list = await svc.SpecsAsync(q);
    return Results.Ok(list.Where(s => s.Active).Select(s => new
    {
        s.Code, s.Name, s.ModelCode, s.SpecType1, s.SpecType2, s.Color,
        s.FlagHasSerial, s.FlagHasLot, s.DefaultUnitCode, s.StandardUnitCode
    }));
});
app.MapGet("/api/specs/{code}/prices", async (string code, IProductService svc) =>
{
    var prices = await svc.SpecPricesAsync(code);
    return Results.Ok(prices.Where(p => p.Active).Select(p => new
    {
        p.UnitCode, p.BuyPrice, p.SellPrice, p.CurrencyCode, p.VatRateCode, p.DiscountVnd,
        effectStart = p.EffectDTimeStart.ToString("yyyy-MM-dd"), effectEnd = p.EffectDTimeEnd.ToString("yyyy-MM-dd")
    }));
});

// Loại hàng hóa (Mst_ProductType) — danh mục dùng chung cho product master.
app.MapGet("/api/product-types", async (string? q, IProductService svc) =>
{
    var list = await svc.ProductTypesAsync(q);
    return Results.Ok(list.Where(t => t.Active).Select(t => new { t.Code, t.Name, t.Remark }));
});

// Loại SSCC (Mst_SSCCType) — danh mục dùng chung cho product master.
app.MapGet("/api/sscc-types", async (string? q, IProductService svc) =>
{
    var list = await svc.SsccTypesAsync(q);
    return Results.Ok(list.Where(s => s.Active).Select(s => new { s.Code, s.Name, s.NetworkId }));
});

app.MapPost("/api/orgs/register", async (RegisterOrgDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest(new { error = "Cần Name." });
    var org = new Org { Name = dto.Name.Trim(), ApiKey = "pim_" + Guid.NewGuid().ToString("N") };
    db.Orgs.Add(org);
    await db.SaveChangesAsync();
    return Results.Ok(new { orgId = org.Id, apiKey = org.ApiKey });
});

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

record RegisterOrgDto(string Name);
