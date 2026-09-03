using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiniPIM.Data;
using MiniPIM.Models;
using MiniPIM.Services;
using Serilog;

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;   // giữ claim gốc (role/name/tenant) từ MiniSSO
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
FleetObs.ConfigureLogger("minipim");

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
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
// SSO chung: tin token do MiniSSO cấp (OIDC RS256). Authority tự nạp discovery + JWKS.
var ssoAuthority = Environment.GetEnvironmentVariable("SSO_AUTHORITY") ?? "https://minisso.onrender.com";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.Authority = ssoAuthority;
    o.RequireHttpsMetadata = ssoAuthority.StartsWith("https");
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = ssoAuthority,
        ValidateAudience = false, ValidateLifetime = true, NameClaimType = "name", RoleClaimType = "role"
    };
});
builder.Services.AddAuthorization();
builder.Services.AddFleetObs();
builder.Services.AddControllersWithViews();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
    await Seeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());

app.UseFleetObs();
app.UseAuthentication();
app.UseAuthorization();

// SSO chung: endpoint xác thực bằng token MiniSSO (đăng nhập 1 lần dùng chung fleet).
app.MapGet("/api/whoami", (ClaimsPrincipal u) => Results.Ok(new
{
    app = "minipim",
    sub = u.FindFirst("sub")?.Value, name = u.Identity?.Name ?? u.FindFirst("name")?.Value,
    email = u.FindFirst("email")?.Value, tenant = u.FindFirst("tenant")?.Value,
    roles = u.FindAll("role").Select(c => c.Value)
})).RequireAuthorization();

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
        attributes = p.Attributes.Select(a => new { a.Name, a.Value }),
        bom = p.Bom.Select(x => new { x.ComponentCode, x.ComponentName, x.Quantity, x.Uom })
    });
});

app.MapPost("/api/orgs/register", async (RegisterOrgDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest(new { error = "Cần Name." });
    var org = new Org { Name = dto.Name.Trim(), ApiKey = "pim_" + Guid.NewGuid().ToString("N") };
    db.Orgs.Add(org);
    await db.SaveChangesAsync();
    return Results.Ok(new { orgId = org.Id, apiKey = org.ApiKey });
});

// Import nhóm sản phẩm thật (dedupe theo Code)
app.MapPost("/api/import/groups", async (List<ImportGroupDto> rows, AppDbContext db, ITenantContext tc) =>
{
    if (rows == null || rows.Count == 0) return Results.BadRequest(new { error = "Không có dữ liệu." });
    int added = 0, skipped = 0;
    var orgId = tc.OrgId;
    var existCodes = db.Groups.Where(g => g.OrgId == orgId).Select(g => g.Code).ToHashSet();
    foreach (var row in rows)
    {
        if (string.IsNullOrWhiteSpace(row.Code)) { skipped++; continue; }
        if (existCodes.Contains(row.Code.Trim())) { skipped++; continue; }
        db.Groups.Add(new ProductGroup { OrgId = orgId, Code = row.Code.Trim(), Name = row.Name?.Trim() ?? row.Code.Trim() });
        existCodes.Add(row.Code.Trim()); added++;
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { added, skipped, total = added + skipped });
});

// Import sản phẩm thật từ DB nguồn (dedupe theo Code)
app.MapPost("/api/import/products", async (List<ImportProdDto> rows, AppDbContext db, ITenantContext tc) =>
{
    if (rows == null || rows.Count == 0) return Results.BadRequest(new { error = "Không có dữ liệu." });
    int added = 0, skipped = 0;
    var orgId = tc.OrgId;
    var existCodes = db.Products.Where(p => p.OrgId == orgId).Select(p => p.Code).ToHashSet();
    foreach (var row in rows)
    {
        if (string.IsNullOrWhiteSpace(row.Code)) { skipped++; continue; }
        var code = row.Code.Trim();
        if (existCodes.Contains(code)) { skipped++; continue; }
        int? groupId = null;
        if (!string.IsNullOrWhiteSpace(row.GroupCode))
        {
            var grp = db.Groups.FirstOrDefault(g => g.OrgId == orgId && g.Code == row.GroupCode.Trim());
            groupId = grp?.Id;
        }
        db.Products.Add(new Product
        {
            OrgId = orgId, Code = code, Name = row.Name?.Trim() ?? code,
            GroupId = groupId, Uom = row.Uom?.Trim() ?? "cái",
            CostPrice = row.CostPrice, SalePrice = row.SalePrice,
            Status = ProductStatus.Active, UpdatedAt = DateTime.Now
        });
        existCodes.Add(code); added++;
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { added, skipped, total = added + skipped });
});

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

record RegisterOrgDto(string Name);
record ImportGroupDto(string? Code, string? Name);
record ImportProdDto(string? Code, string? Name, string? GroupCode, string? Uom, decimal CostPrice, decimal SalePrice);
