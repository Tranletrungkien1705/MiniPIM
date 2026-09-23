namespace MiniPIM.Models;

public class Org
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
public interface IOrgOwned { Guid OrgId { get; set; } }

public enum ProductStatus { Active = 0, Inactive = 1 }

/// <summary>Cấp sản phẩm trong hệ thống (ProductLevelSys của ProductCenter).</summary>
public enum ProductLevel { Root = 0, Base = 1, L2 = 2 }

/// <summary>Danh mục thuộc tính dùng chung (Mst_Attribute) — sản phẩm tham chiếu theo mã.</summary>
public class AttributeDef : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool Active { get; set; } = true;
}

/// <summary>
/// Đơn vị tính (Mst_Unit của ProductCenter) — danh mục ĐVT dùng chung cho sản phẩm,
/// quy cách và bảng giá. UnitCode là mã hệ thống (tự sinh), UnitCodeUser là mã
/// người dùng nhập; cả hai cùng UnitName đều duy nhất trong một tổ chức.
/// </summary>
public class Unit : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // UnitCode — mã hệ thống (tự sinh)
    public string CodeUser { get; set; } = "";    // UnitCodeUser — mã người dùng nhập
    public string Name { get; set; } = "";        // UnitName
    public string? Remark { get; set; }            // Remark — ghi chú
    public bool Active { get; set; } = true;       // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Thuế suất GTGT (Mst_VATRate của ProductCenter) — danh mục dùng chung cho
/// sản phẩm (Product.VatRateCode) và bảng giá theo quy cách (SpecPrice.VatRateCode).
/// VATRateCode là mã hệ thống, VATRate là giá trị %, VATDesc là mô tả/tên hiển thị.
/// </summary>
public class VatRate : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // VATRateCode — mã thuế suất
    public decimal Rate { get; set; }              // VATRate — giá trị %
    public string Name { get; set; } = "";        // VATDesc — mô tả/tên hiển thị
    public bool Active { get; set; } = true;       // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Nhãn hiệu / Thương hiệu (Mst_Brand của ProductCenter) — danh mục dùng chung
/// cho product master: nhóm hàng tham chiếu theo BrandCode. BrandCode là mã hệ
/// thống (duy nhất trong tổ chức), BrandName là tên hiển thị, NetworkBrandCode
/// là mã nhãn hiệu chung của network (dùng khi đồng bộ giữa các tổ chức).
/// </summary>
public class Brand : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // BrandCode — mã nhãn hiệu
    public string Name { get; set; } = "";        // BrandName — tên nhãn hiệu
    public string? NetworkBrandCode { get; set; }  // NetworkBrandCode — mã nhãn hiệu chung network
    public string? Remark { get; set; }            // Remark — ghi chú
    public bool Active { get; set; } = true;       // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Nhóm hàng (Mst_ProductGroup của ProductCenter) — phân cấp cha/con.
/// BUCode/BUPattern/Level là "đường dẫn vật chất hóa" (materialized path) được
/// tính lại tự động từ cây cha/con (nghiệp vụ Mst_ProductGroup_UpdBU).
/// </summary>
public class ProductGroup : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";

    // --- Phân cấp & đường dẫn (Mst_ProductGroup) ---
    public string? ParentCode { get; set; }        // ProductGrpCodeParent
    public string? BrandCode { get; set; }         // BrandCode — nhãn hiệu của nhóm (Mst_Brand)
    public string BUCode { get; set; } = "";      // ProductGrpBUCode — đường dẫn mã, vd "ALL.AO.SOMI"
    public string BUPattern { get; set; } = "";    // ProductGrpBUPattern — dùng LIKE để lấy cả cây con
    public int Level { get; set; }                 // ProductGrpLevel — 0 = gốc
    public bool FlagFG { get; set; }               // FlagFG — nhóm thành phẩm
    public bool Active { get; set; } = true;       // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>Sản phẩm master (PIM) — nguồn dữ liệu chuẩn cấp cho DMS/WMS/Stamp.</summary>
public class Product : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // SKU
    public string Name { get; set; } = "";
    public int? GroupId { get; set; }
    public string Uom { get; set; } = "cái";       // đơn vị tính
    public string? Barcode { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;

    // --- Trường master bổ sung theo ProductCenter (TblMst_Product) ---
    public ProductLevel Level { get; set; } = ProductLevel.Base;   // ProductLevelSys
    public decimal ValConvert { get; set; } = 1;                    // hệ số quy đổi ĐVT
    public decimal QtyMinSt { get; set; }                           // tồn tối thiểu
    public decimal QtyMaxSt { get; set; }                           // tồn tối đa
    public string? VatRateCode { get; set; }                        // mã thuế GTGT
    public bool FlagSerial { get; set; }                            // quản lý theo serial
    public bool FlagLot { get; set; }                               // quản lý theo lô
    public string? Origin { get; set; }                             // xuất xứ (ProductOrigin)
    public string? QuyCach { get; set; }                            // quy cách đóng gói (ProductQuyCach)

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public ProductGroup? Group { get; set; }
    public List<ProductAttribute> Attributes { get; set; } = [];
    public List<BomLine> Bom { get; set; } = [];
}

/// <summary>Thuộc tính động (dynamic field) — khác nhau theo loại SP.</summary>
public class ProductAttribute : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public string? Value { get; set; }
    public Product Product { get; set; } = null!;
}

/// <summary>Định mức nguyên vật liệu (BOM): 1 thành phẩm gồm các thành phần.</summary>
public class BomLine : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ProductId { get; set; }          // thành phẩm
    public string ComponentCode { get; set; } = "";
    public string ComponentName { get; set; } = "";
    public decimal Quantity { get; set; } = 1;
    public string Uom { get; set; } = "cái";
    public Product Product { get; set; } = null!;
}

/// <summary>
/// Quy cách sản phẩm (Mst_Spec của ProductCenter) — biến thể bán được của một model:
/// màu, loại 1/2, quản lý serial/lô, ĐVT mặc định & ĐVT chuẩn.
/// </summary>
public class Spec : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // SpecCode
    public string Name { get; set; } = "";        // SpecName
    public string? Description { get; set; }       // SpecDesc
    public string? ModelCode { get; set; }         // ModelCode
    public string? SpecType1 { get; set; }         // SpecType1
    public string? SpecType2 { get; set; }         // SpecType2
    public string? Color { get; set; }             // Color
    public bool FlagHasSerial { get; set; }        // FlagHasSerial
    public bool FlagHasLot { get; set; }           // FlagHasLOT
    public string? DefaultUnitCode { get; set; }   // DefaultUnitCode
    public string? StandardUnitCode { get; set; }  // StandardUnitCode
    public string? Remark { get; set; }
    public bool Active { get; set; } = true;       // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<SpecPrice> Prices { get; set; } = [];
}

/// <summary>
/// Bảng giá theo quy cách (Mst_SpecPrice) — khóa nghiệp vụ (SpecCode, UnitCode).
/// Giá mua/giá bán, tiền tệ, thuế GTGT, chiết khấu và khoảng hiệu lực.
/// </summary>
public class SpecPrice : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int SpecId { get; set; }
    public string SpecCode { get; set; } = "";
    public string UnitCode { get; set; } = "";    // ĐVT áp giá
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public string? VatRateCode { get; set; }
    public decimal DiscountVnd { get; set; }
    public DateTime EffectDTimeStart { get; set; } = DateTime.Now;
    public DateTime EffectDTimeEnd { get; set; } = new DateTime(2100, 1, 1);
    public string? Remark { get; set; }
    public bool Active { get; set; } = true;

    public Spec Spec { get; set; } = null!;
}
