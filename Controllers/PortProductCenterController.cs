using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniPIM.Data;
using MiniPIM.Models;

namespace MiniPIM.Controllers;

/// <summary>
/// Endpoint port 1:1 các màn lõi sản phẩm của dự án nguồn <b>2019.4.ProductCenter</b>
/// (đường dẫn nguồn: <c>2019.4.ProductCenter/Dev/V20/ProductCenter.Web</c>).
///
/// Ba màn đã port ở đây:
///  1. <c>Views/Mst_Product/SearchProduct.cshtml</c>      — lưới tìm hàng hoá (bảng Mst_Product)
///  2. <c>Views/Mst_Product/ProductOtherInformation.cshtml</c> — tab "Thông tin khác" (bảng Mst_Product)
///  3. <c>Views/Mst_Product/ProductIngredient.cshtml</c>   — thành phần / định mức (bảng Prd_BOM)
///
/// Điều kiện lọc bám đúng hàm dựng where của nguồn, KHÔNG tự chế:
///  • <c>Mst_ProductController.cs:4845-4922</c>  strWhereClause_Mst_Product_Search
///  • <c>Mst_ProductController.cs:4985-5047</c>  strWhereClause_Mst_ProductProductUserCodeOrProductName
///  • <c>Mst_ProductController.cs:1216-1243</c>  gom checkbox Sản phẩm/Dịch vụ/Combo thành ProductType in (...)
/// </summary>
[ApiController]
[Route("api/pc")]
[Produces("application/json")]
public class PortProductCenterController(AppDbContext db) : ControllerBase
{
    // ===================================================================================
    // MÀN 1 — SearchProduct.cshtml  (port 1:1)
    // ===================================================================================

    /// <summary>
    /// Lưới tìm hàng hoá. Port 1:1 <c>Mst_ProductController.GetSearch</c> (nguồn dòng 1183-1292)
    /// + view <c>SearchProduct.cshtml</c> (12 cột lưới, dòng 53-86).
    ///
    /// Thứ tự guard đúng như nguồn:
    ///  • ProductGrpCode / ProductLevelSys / BrandCode / UnitCode / FlagActive  → so "=" (:4858-4876)
    ///  • ProductCodeUser "=" HOẶC ProductName "like %…%" bọc trong 1 ngoặc      (:4888-4908)
    ///  • ProductType in ('PRODUCT','SERVICE','COMBO') theo checkbox            (:4910-4919)
    ///  • KHÔNG tick checkbox nào ⇒ nguồn tự ép FlagActive='1'                  (:1240-1243)
    ///  • Thuộc tính động: AttributeCode "=" và AttributeValue "like %…%"       (:4878-4884)
    /// </summary>
    [HttpGet("searchproduct")]
    public async Task<IActionResult> SearchProduct(
        [FromQuery] string? productCodeUserName,
        [FromQuery] string? productGrpCode,
        [FromQuery] string? brandCode,
        [FromQuery] string? unitCode,
        [FromQuery] string? productLevelSys,
        [FromQuery] string? ckbProduct,
        [FromQuery] string? ckbServices,
        [FromQuery] string? ckbCombo,
        [FromQuery] string? attributeCode,
        [FromQuery] string? attributeValue,
        [FromQuery] int recordCount = 100)
    {
        // Gom 3 checkbox thành danh sách ProductType — nguồn Mst_ProductController.cs:1216-1230.
        var dsLoai = new List<string>();
        if (ckbProduct == "1") dsLoai.Add("PRODUCT");
        if (ckbServices == "1") dsLoai.Add("SERVICE");
        if (ckbCombo == "1") dsLoai.Add("COMBO");

        var q = db.Products.Include(p => p.Group).Include(p => p.Attributes).AsQueryable();

        if (!string.IsNullOrWhiteSpace(productGrpCode))
            q = q.Where(p => p.Group != null && p.Group.Code == productGrpCode);       // ProductGrpCode "="
        if (!string.IsNullOrWhiteSpace(productLevelSys))
            q = q.Where(p => p.LevelSys == productLevelSys);                            // ProductLevelSys "="
        if (!string.IsNullOrWhiteSpace(brandCode))
            q = q.Where(p => p.BrandCode == brandCode);                                 // BrandCode "="
        if (!string.IsNullOrWhiteSpace(unitCode))
            q = q.Where(p => p.Uom == unitCode);                                        // UnitCode "=" (đích đổi tên Uom)

        // Nguồn: không tick checkbox nào thì ép FlagActive='1' (Mst_ProductController.cs:1240-1243).
        // Bên đích FlagActive ↔ Status (0 = Active) — xem bảng đối chiếu trong AGENT_MiniPIM.md.
        if (dsLoai.Count == 0)
            q = q.Where(p => p.Status == ProductStatus.Active);
        else
            q = q.Where(p => p.ProductType != null && dsLoai.Contains(p.ProductType));  // ProductType in (...)

        // Một ô nhập duy nhất, nguồn gán vào CẢ ProductCodeUser và ProductName rồi OR hai điều kiện
        // (Mst_ProductController.cs:1232-1237 + :4888-4908). ProductCodeUser so "=", ProductName so "like".
        if (!string.IsNullOrWhiteSpace(productCodeUserName))
        {
            var k = productCodeUserName.Trim();
            // PHẢI dùng EF.Functions.Like, KHÔNG dùng string.Contains: trên SQLite EF dịch Contains
            // thành instr() -> PHÂN BIỆT hoa/thường, còn nguồn là SQL Server `like` với collation
            // mặc định KHÔNG phân biệt. Đã bắt được bằng test thật: tìm "loc" trả 0 dòng dù có "PT-LOC-DAU".
            q = q.Where(p => p.CodeUser == k || EF.Functions.Like(p.Name ?? "", "%" + k + "%"));
        }

        // Thuộc tính động: nguồn AND cặp (AttributeCode "=", AttributeValue "like %…%")
        // — Mst_ProductController.cs:4878-4884. Bên đích AttributeCode ↔ ProductAttribute.Name.
        if (!string.IsNullOrWhiteSpace(attributeCode))
        {
            var ac = attributeCode.Trim();
            var av = (attributeValue ?? "").Trim();
            q = q.Where(p => p.Attributes.Any(a => a.Name == ac
                && (av == "" || EF.Functions.Like(a.Value ?? "", "%" + av + "%"))));
        }

        var ds = await q.OrderBy(p => p.CodeUser ?? p.Code).Take(recordCount <= 0 ? 100 : recordCount).ToListAsync();

        // 12 cột lưới đúng thứ tự SearchProduct.cshtml:53-86 (CONTROL/STT là UI, không phải cột dữ liệu).
        return Ok(new
        {
            itemCount = ds.Count,
            items = ds.Select((p, i) => new
            {
                stt = i + 1,
                productCode = p.Code,                    // hidden ListProduct[i].ProductCode  (:100)
                productCodeUser = p.CodeUser,            // cột "Mã hàng"                      (:121)
                productName = p.Name,                    // cột "Tên hàng"                     (:124)
                productType = p.ProductType,              // cột "Loại hàng"                    (:127)
                productGrpCode = p.Group?.Code,           // cột "Nhóm hàng"                    (:130)
                productGrpName = p.Group?.Name,
                brandCode = p.BrandCode,                  // cột "Nhãn hiệu"                    (:133)
                unitCode = p.Uom,                         // cột "Đơn vị"                       (:136)
                flagBuy = p.FlagBuy,                      // cột "Được mua"  '1'/'0'            (:139)
                flagSell = p.FlagSell,                    // cột "Được bán"  '1'/'0'            (:153)
                uPBuy = p.CostPrice,                      // cột "Giá mua"                      (:164)
                uPSell = p.SalePrice,                     // cột "Giá bán"                      (:167)
                productLevelSys = p.LevelSys,             // hidden                             (:106)
                productCodeBase = p.CodeBase,             // hidden                             (:107)
                productCodeRoot = p.CodeRoot,             // hidden                             (:108)
                qtyEffSt = p.QtyEffSt,                    // hidden                             (:113)
                orgID = p.OrgId                           // hidden                             (:109)
            })
        });
    }

    /// <summary>
    /// Autocomplete ô "Nhập mã hàng hoặc tên hàng để tìm kiếm" của màn thành phần.
    /// Port 1:1 <c>Mst_ProductController.Search_Product_Auto</c> (:469-537) — tối đa 10 bản ghi
    /// (nguồn hard-code <c>Ft_RecordCount = "10"</c>, :505 kèm comment "hỗ trợ lấy tối đa 10 bản ghi"),
    /// và luôn ép <c>FlagActive = '1'</c> (:491).
    /// </summary>
    [HttpGet("searchproductauto")]
    public async Task<IActionResult> SearchProductAuto([FromQuery] string? productcodename)
    {
        if (string.IsNullOrWhiteSpace(productcodename))
            return Ok(new { success = true, listProduct = Array.Empty<object>() });     // nguồn :482 chỉ chạy khi có keyword

        var k = productcodename.Trim();
        var ds = await db.Products.Include(p => p.Group)
            .Where(p => p.Status == ProductStatus.Active)                                // FlagActive = '1'
            // :5030-5031 ProductCodeUser like '%k%' OR ProductName like '%k%'.
            // Dùng EF.Functions.Like để giữ đúng tính KHÔNG phân biệt hoa/thường của `like` SQL Server.
            .Where(p => EF.Functions.Like(p.CodeUser ?? "", "%" + k + "%")
                     || EF.Functions.Like(p.Name ?? "", "%" + k + "%"))
            .OrderBy(p => p.CodeUser ?? p.Code).Take(10).ToListAsync();

        return Ok(new
        {
            success = true,
            listProduct = ds.Select(p => new
            {
                productCode = p.Code, productCodeUser = p.CodeUser, productName = p.Name,
                productType = p.ProductType, productCodeRoot = p.CodeRoot,
                uPBuy = p.CostPrice, uPSell = p.SalePrice, unitCode = p.Uom,
                qtyEffSt = p.QtyEffSt                                                     // :398-403 null ⇒ mặc định 1 ở FE
            })
        });
    }

    // ===================================================================================
    // MÀN 2 — ProductOtherInformation.cshtml  (port 1:1)
    // ===================================================================================

    /// <summary>Đọc tab "Thông tin khác" theo mã hàng. 12 trường của ProductOtherInformation.cshtml.</summary>
    [HttpGet("productotherinformation/{code}")]
    public async Task<IActionResult> GetOtherInformation(string code)
    {
        var p = await db.Products.FirstOrDefaultAsync(x => x.Code == code || x.CodeUser == code);
        if (p == null) return NotFound(new { error = "Không tìm thấy hàng hoá." });
        return Ok(ThongTinKhac(p));
    }

    /// <summary>
    /// Ghi tab "Thông tin khác". Các ô nguồn đều <c>required</c> ở HTML
    /// (ProductOtherInformation.cshtml:18,27,74,83,96,105) nhưng nguồn KHÔNG có guard server-side nào
    /// cho nhóm này — nên bên đích chỉ chặn thiếu mã hàng, không tự thêm luật mới.
    /// </summary>
    [HttpPost("productotherinformation")]
    public async Task<IActionResult> SaveOtherInformation([FromBody] OtherInfoReq r)
    {
        if (string.IsNullOrWhiteSpace(r.Code)) return BadRequest(new { error = "Cần mã hàng." });
        var p = await db.Products.FirstOrDefaultAsync(x => x.Code == r.Code || x.CodeUser == r.Code);
        if (p == null) return NotFound(new { error = "Không tìm thấy hàng hoá." });

        p.NameEn = r.ProductNameEN; p.Std = r.ProductStd; p.Origin = r.ProductOrigin;
        p.Expiry = r.ProductExpiry; p.QuyCach = r.ProductQuyCach; p.MnfUrl = r.ProductMnfUrl;
        p.Intro = r.ProductIntro; p.UserGuide = r.ProductUserGuide; p.Drawing = r.ProductDrawing;
        // Cờ nguồn là '1'/'0' — checkbox không tick gửi rỗng, nguồn coi rỗng là khác "1" (:38,53).
        p.FlagSerial = r.FlagSerial == "1" ? "1" : "0";
        p.FlagLot = r.FlagLot == "1" ? "1" : "0";
        // Remark ↔ Description bên đích (đổi tên). Ô này bị COMMENT ở nguồn (:110-118) — xem mục 5 báo cáo.
        p.Description = r.Remark ?? p.Description;
        p.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return Ok(ThongTinKhac(p));
    }

    private static object ThongTinKhac(Product p) => new
    {
        productCode = p.Code, productCodeUser = p.CodeUser, productName = p.Name,
        productNameEN = p.NameEn,          // ProductNameEN  — "Tên tiếng anh"
        productStd = p.Std,                // ProductStd     — "Tiêu chuẩn"
        productOrigin = p.Origin,          // ProductOrigin  — "Xuất xứ"
        productExpiry = p.Expiry,          // ProductExpiry  — "Hạn sử dụng"
        productQuyCach = p.QuyCach,        // ProductQuyCach — "Quy cách"
        productMnfUrl = p.MnfUrl,          // ProductMnfUrl  — "Link quy trình XS"
        productIntro = p.Intro,            // ProductIntro   — tab "Chi tiết sản phẩm"
        productUserGuide = p.UserGuide,    // ProductUserGuide — tab "Hướng dẫn sử dụng"
        productDrawing = p.Drawing,        // ProductDrawing — tab "Bản vẽ"
        flagSerial = p.FlagSerial,         // FlagSerial     — "Quản lý Serial"
        flagLot = p.FlagLot,               // FlagLot        — "Quản lý LOT"
        remark = p.Description             // Remark         — ô bị comment ở nguồn, đích đổi tên Description
    };

    // ===================================================================================
    // MÀN 3 — ProductIngredient.cshtml  (port 1:1, bảng Prd_BOM)
    // ===================================================================================

    /// <summary>
    /// Lưới thành phần (BOM) của một thành phẩm. Port 1:1 <c>ProductIngredient.cshtml:84-121</c>
    /// + <c>Mst_ProductController.GetBOM</c> (:1068-1121).
    /// Hai cột tính: Tổng tiền mua = mp_UPBuy × Qty (:95), Tổng tiền bán = mp_UPSell × Qty (:96).
    /// </summary>
    [HttpGet("productingredient/{code}")]
    public async Task<IActionResult> GetIngredient(string code)
    {
        var p = await db.Products.Include(x => x.Bom)
            .FirstOrDefaultAsync(x => x.Code == code || x.CodeUser == code);
        if (p == null) return NotFound(new { error = "Không tìm thấy hàng hoá." });

        return Ok(new
        {
            productCode = p.Code, productCodeUser = p.CodeUser, productName = p.Name,
            productCodeRoot = p.CodeRoot,
            listBOM = p.Bom.Where(b => b.FlagActive == "1").Select((b, i) => new
            {
                idx = i + 1,
                productCode = b.ComponentCode,             // Prd_BOM.ProductCode      (thành phần)
                mp_ProductCodeUser = b.ComponentCode,       // mp_ProductCodeUser       (:104)
                mp_ProductName = b.ComponentName,           // mp_ProductName           (:105)
                qty = b.Quantity,                           // Prd_BOM.Qty              (:106)
                mp_UPBuy = b.ComponentCostPrice,            // mp_UPBuy                 (:107)
                uPBuyAmount = b.CostAmount,                 // = mp_UPBuy × Qty         (:95)
                mp_UPSell = b.ComponentSalePrice,           // mp_UPSell                (:109)
                uPSellAmount = b.SaleAmount,                // = mp_UPSell × Qty        (:96)
                productCodeParent = b.ParentCodeRoot,       // Prd_BOM.ProductCodeParent(:114)
                unitCode = b.Uom,
                flagActive = b.FlagActive
            })
        });
    }

    /// <summary>
    /// Ghi toàn bộ lưới thành phần (replace-all, giống nguồn: nguồn dựng lại cả
    /// <c>Lst_Prd_BOM</c> mỗi lần lưu — Mst_ProductController.cs:1992-2010, 2031, 2045-2048).
    ///
    /// Guard port 1:1:
    ///  • CẤM trùng thành phần trên lưới — <c>CheckProductTable</c> (ProductIngredient.cshtml:359-390),
    ///    thông báo nguồn: 'Hàng "&lt;tên&gt;" đã tồn tại trên lưới!' (:379).
    ///  • Qty null ⇒ nguồn gán "" (:91-94); khi thêm dòng mới lấy QtyEffSt, null thì 1 (:398-403).
    ///  • ProductCodeParent = ProductCodeRoot của thành phẩm (Mst_ProductController.cs:2002).
    /// </summary>
    [HttpPost("productingredient")]
    public async Task<IActionResult> SaveIngredient([FromBody] IngredientReq r)
    {
        if (string.IsNullOrWhiteSpace(r.ProductCode)) return BadRequest(new { error = "Cần mã hàng thành phẩm." });
        var p = await db.Products.Include(x => x.Bom)
            .FirstOrDefaultAsync(x => x.Code == r.ProductCode || x.CodeUser == r.ProductCode);
        if (p == null) return NotFound(new { error = "Không tìm thấy hàng hoá." });

        var dong = r.ListBOM ?? [];

        // Guard trùng — nguồn chặn ở FE (CheckProductTable :359-390). Bên đích chặn lại ở server
        // vì endpoint là bề mặt ghi thật, FE có thể bị bỏ qua.
        var trung = dong.Where(x => !string.IsNullOrWhiteSpace(x.ProductCode))
            .GroupBy(x => x.ProductCode!.Trim()).FirstOrDefault(g => g.Count() > 1);
        if (trung != null)
            return BadRequest(new { error = $"Hàng \"{trung.First().ProductName ?? trung.Key}\" đã tồn tại trên lưới!" });

        // Cấm lấy chính thành phẩm làm thành phần của nó (nguồn không có guard này — xem mục 5 báo cáo).
        if (dong.Any(x => (x.ProductCode ?? "").Trim() == p.Code))
            return BadRequest(new { error = "Không được đưa chính hàng hoá này vào lưới thành phần." });

        db.BomLines.RemoveRange(p.Bom);

        var moi = new List<BomLine>();
        foreach (var x in dong)
        {
            var maTP = (x.ProductCode ?? "").Trim();
            if (maTP.Length == 0) continue;
            // Giá thành phần LUÔN đọc lại từ Mst_Product, không tin số FE gửi lên.
            var tp = await db.Products.FirstOrDefaultAsync(y => y.Code == maTP || y.CodeUser == maTP);
            if (tp == null) return BadRequest(new { error = $"Thành phần \"{maTP}\" không có trong danh mục." });

            moi.Add(new BomLine
            {
                ProductId = p.Id,
                ComponentCode = tp.Code,
                ComponentName = tp.Name,
                // Qty: nguồn để rỗng thì lấy QtyEffSt, QtyEffSt null thì 1 (ProductIngredient.cshtml:398-403).
                Quantity = x.Qty.HasValue && x.Qty.Value > 0 ? x.Qty.Value : (tp.QtyEffSt ?? 1),
                Uom = tp.Uom,
                ComponentCostPrice = tp.CostPrice,       // mp_UPBuy
                ComponentSalePrice = tp.SalePrice,       // mp_UPSell
                ParentCodeRoot = p.CodeRoot ?? p.Code,   // = ProductCodeRoot của thành phẩm
                FlagActive = "1"
            });
        }
        p.Bom = moi;
        p.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return await GetIngredient(p.Code);
    }
}

/// <summary>Request tab "Thông tin khác" — tên field giữ đúng tên cột nguồn Mst_Product.</summary>
public class OtherInfoReq
{
    public string? Code { get; set; }
    public string? ProductNameEN { get; set; }
    public string? ProductStd { get; set; }
    public string? ProductOrigin { get; set; }
    public string? ProductExpiry { get; set; }
    public string? ProductQuyCach { get; set; }
    public string? ProductMnfUrl { get; set; }
    public string? ProductIntro { get; set; }
    public string? ProductUserGuide { get; set; }
    public string? ProductDrawing { get; set; }
    public string? FlagSerial { get; set; }
    public string? FlagLot { get; set; }
    public string? Remark { get; set; }
}

/// <summary>Request lưới thành phần — cấu trúc theo <c>ListBOM[i].*</c> của nguồn (ProductIngredient.cshtml:111-117).</summary>
public class IngredientReq
{
    public string? ProductCode { get; set; }
    public List<PcBomReq>? ListBOM { get; set; }
}
public class PcBomReq
{
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public decimal? Qty { get; set; }
}
