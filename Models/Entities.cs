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

/// <summary>
/// Danh mục thuộc tính dùng chung (Mst_Attribute của ProductCenter) — sản phẩm tham chiếu
/// theo mã. AttributeCode là mã hệ thống (duy nhất trong tổ chức), AttributeName là tên
/// hiển thị (không trùng trong cùng NetworkID), NetworkID là mã dùng chung của network
/// (đồng bộ giữa các tổ chức).
/// </summary>
public class AttributeDef : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // AttributeCode — mã thuộc tính
    public string Name { get; set; } = "";        // AttributeName — tên thuộc tính
    public string? NetworkId { get; set; }          // NetworkID — mã dùng chung network
    public bool Active { get; set; } = true;       // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
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
/// Model / Dòng sản phẩm (Mst_Model của ProductCenter) — danh mục dùng chung cho
/// product master: quy cách (Spec.ModelCode) và sản phẩm tham chiếu theo ModelCode.
/// ModelCode là mã hệ thống (duy nhất trong tổ chức), ModelName là tên hiển thị,
/// BrandCode là nhãn hiệu (Mst_Brand) mà model thuộc về, NetworkModelCode là mã
/// model chung của network (dùng khi đồng bộ giữa các tổ chức).
/// </summary>
public class Model : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // ModelCode — mã model
    public string Name { get; set; } = "";        // ModelName — tên model
    public string? OrgModelCode { get; set; }      // OrgModelCode — mã model theo tổ chức
    public string? BrandCode { get; set; }         // BrandCode — nhãn hiệu (Mst_Brand)
    public string? NetworkModelCode { get; set; }  // NetworkModelCode — mã model chung network
    public string? Remark { get; set; }            // Remark — ghi chú
    public bool Active { get; set; } = true;       // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Loại hàng hóa (Mst_ProductType của ProductCenter) — danh mục dùng chung cho
/// product master: sản phẩm tham chiếu theo ProductType. ProductType là mã hệ thống
/// (duy nhất trong tổ chức), ProductTypeName là tên hiển thị. Loại "COMBO" (TConst.ProductType.Combo)
/// đánh dấu hàng hóa là combo (bán theo bộ thành phần).
/// </summary>
public class ProductType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // ProductType — mã loại hàng hóa
    public string Name { get; set; } = "";        // ProductTypeName — tên loại hàng hóa
    public string? Remark { get; set; }            // Remark — ghi chú
    public bool Active { get; set; } = true;       // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Tỷ giá ngoại tệ (Mst_CurrencyEx của ProductCenter) — danh mục dùng chung cho
/// product master: bảng giá theo quy cách (SpecPrice.CurrencyCode) quy đổi về đồng
/// tiền gốc. CurrencyCode là mã ngoại tệ (duy nhất trong tổ chức), CurrencyName là
/// tên hiển thị, BaseCurrencyCode là đồng tiền gốc quy đổi (phải tồn tại),
/// BuyRate/SellRate là tỷ giá mua/bán, InterEx là tỷ giá liên ngân hàng.
/// </summary>
public class CurrencyEx : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // CurrencyCode — mã ngoại tệ
    public string Name { get; set; } = "";        // CurrencyName — tên ngoại tệ
    public string? BaseCurrencyCode { get; set; }  // BaseCurrencyCode — đồng tiền gốc quy đổi
    public decimal BuyRate { get; set; }           // BuyRate — tỷ giá mua
    public decimal SellRate { get; set; }          // SellRate — tỷ giá bán
    public decimal InterEx { get; set; }           // InterEx — tỷ giá liên ngân hàng
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
    public string? ProductTypeCode { get; set; }                    // ProductType — loại hàng hóa (Mst_ProductType)
    public string? SsccTypeCode { get; set; }                       // SSCCType — loại SSCC (Mst_SSCCType)
    public string? Gtin { get; set; }                               // GTIN — mã thương phẩm toàn cầu (phải là số)

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
/// <summary>
/// Đơn vị tính theo quy cách (Mst_SpecUnit của ProductCenter) — khóa nghiệp vụ
/// (SpecCode, UnitCode). Mỗi quy cách có nhiều ĐVT bán được, kèm hệ số quy đổi
/// (Qty) và kích thước/khối lượng đóng gói (Length/Width/Height/Volume/Weight)
/// để tính vận chuyển. StandardUnitCode là ĐVT chuẩn quy đổi về.
/// </summary>
public class SpecUnit : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string SpecCode { get; set; } = "";        // SpecCode — quy cách (Mst_Spec)
    public string UnitCode { get; set; } = "";        // UnitCode — ĐVT (Mst_Unit)
    public string? StandardUnitCode { get; set; }      // StandardUnitCode — ĐVT chuẩn quy đổi về
    public string? Description { get; set; }           // SpecUnitDesc — mô tả
    public decimal Qty { get; set; } = 1;              // Qty — hệ số quy đổi
    public decimal? Length { get; set; }               // Length — dài
    public decimal? Width { get; set; }                // Width — rộng
    public decimal? Height { get; set; }               // Height — cao
    public decimal? Volume { get; set; }               // Volume — thể tích
    public decimal? Weight { get; set; }               // Weight — khối lượng
    public string? Remark { get; set; }                // Remark — ghi chú
    public bool Active { get; set; } = true;           // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Loại quy cách (Mst_SpecType1 / Mst_SpecType2 của ProductCenter) — danh mục dùng
/// chung cho product master: quy cách (Spec.SpecType1/SpecType2) tham chiếu theo mã.
/// Hai bảng nguồn có cấu trúc giống hệt nhau nên gộp thành 1 entity với Kind = 1|2
/// (1 = SpecType1, 2 = SpecType2). Code là mã loại (duy nhất trong tổ chức theo Kind),
/// Name là tên hiển thị, NetworkId là mã dùng chung của network (đồng bộ giữa tổ chức).
/// </summary>
public class SpecType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int Kind { get; set; } = 1;             // 1 = SpecType1, 2 = SpecType2
    public string Code { get; set; } = "";        // SpecType1 / SpecType2 — mã loại quy cách
    public string Name { get; set; } = "";        // SpecType1Name / SpecType2Name — tên loại
    public string? NetworkId { get; set; }          // NetworkID — mã dùng chung network
    public string? Remark { get; set; }             // Remark — ghi chú
    public bool Active { get; set; } = true;        // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Loại SSCC (Mst_SSCCType của ProductCenter) — danh mục dùng chung cho product master:
/// hàng hóa tham chiếu theo SSCCType (mã loại SSCC) để khai báo đơn vị đóng gói logistics
/// (thùng/pallet theo chuẩn SSCC — Serial Shipping Container Code). SSCCType là mã hệ thống
/// (duy nhất trong tổ chức), SSCCTypeName là tên hiển thị, NetworkID là mã dùng chung của
/// network (đồng bộ giữa các tổ chức).
/// </summary>
public class SsccType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // SSCCType — mã loại SSCC
    public string Name { get; set; } = "";        // SSCCTypeName — tên loại SSCC
    public string? NetworkId { get; set; }          // NetworkID — mã dùng chung network
    public bool Active { get; set; } = true;        // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Trường tùy chỉnh của quy cách (Mst_SpecCustomField của ProductCenter) — danh mục
/// định nghĩa các trường động gắn cho quy cách (Mst_Spec). SpecCustomFieldCode là mã
/// hệ thống (duy nhất trong tổ chức), SpecCustomFieldName là tên hiển thị,
/// DBPhysicalType là kiểu dữ liệu vật lý lưu trong DB (mặc định nvarchar(400)),
/// NetworkID là mã dùng chung của network (đồng bộ giữa các tổ chức).
/// </summary>
public class SpecCustomField : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // SpecCustomFieldCode — mã trường tùy chỉnh
    public string Name { get; set; } = "";        // SpecCustomFieldName — tên trường tùy chỉnh
    public string? NetworkId { get; set; }          // NetworkID — mã dùng chung network
    public string? DBPhysicalType { get; set; }     // DBPhysicalType — kiểu dữ liệu vật lý (mặc định nvarchar(400))
    public string? Remark { get; set; }             // Remark — ghi chú
    public bool Active { get; set; } = true;        // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Ảnh của hàng hóa (Mst_ProductImages của ProductCenter) — một hàng hóa có nhiều ảnh,
/// mỗi ảnh có thứ tự (Idx), đường dẫn (ProductImagePath), tên (ProductImageName),
/// mô tả (ProductImageDesc) và cờ ảnh chính (FlagPrimaryImage). ProductCode là khóa
/// nghiệp vụ trỏ tới Hàng hóa master (Mst_Product).
/// </summary>
public class ProductImage : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ProductCode { get; set; } = "";  // ProductCode — hàng hóa (Mst_Product)
    public int Idx { get; set; }                    // Idx — thứ tự hiển thị
    public string? NetworkId { get; set; }          // NetworkID — mã dùng chung network
    public string? ImagePath { get; set; }          // ProductImagePath — đường dẫn ảnh
    public string? ImageName { get; set; }          // ProductImageName — tên ảnh
    public string? ImageDesc { get; set; }          // ProductImageDesc — mô tả ảnh
    public bool FlagPrimaryImage { get; set; }      // FlagPrimaryImage — ảnh chính
    public bool Active { get; set; } = true;        // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Tệp/tài liệu đính kèm của hàng hóa (Mst_ProductFiles của ProductCenter) — một hàng hóa
/// có nhiều tệp (bản vẽ, hướng dẫn sử dụng, chứng từ), mỗi tệp có thứ tự (Idx), đường dẫn
/// (ProductFilePath), tên (ProductFileName) và mô tả (ProductFileDesc). ProductCode là
/// khóa nghiệp vụ trỏ tới Hàng hóa master (Mst_Product).
/// </summary>
public class ProductFile : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ProductCode { get; set; } = "";  // ProductCode — hàng hóa (Mst_Product)
    public int Idx { get; set; }                    // Idx — thứ tự hiển thị
    public string? NetworkId { get; set; }          // NetworkID — mã dùng chung network
    public string? FilePath { get; set; }           // ProductFilePath — đường dẫn tệp
    public string? FileName { get; set; }           // ProductFileName — tên tệp
    public string? FileDesc { get; set; }           // ProductFileDesc — mô tả tệp
    public bool Active { get; set; } = true;        // FlagActive
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}