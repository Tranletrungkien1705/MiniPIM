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

/// <summary>Nhóm sản phẩm (phân cấp 1 tầng cho demo).</summary>
public class ProductGroup : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
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
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ----- Port 1:1 từ 2019.4.ProductCenter — bảng Mst_Product -----
    // Tên cột DB thật lấy ở ProductCenter.Constants/Const.Main.cs:1096-1141 (class TblMst_Product),
    // kiểu khai báo ở ProductCenter.Common/Models/Mst_Product.cs:11-83.

    /// <summary>ProductCodeUser — "Mã hàng" người dùng nhìn thấy trên lưới.
    /// Nguồn có HAI mã: ProductCode (mã hệ thống, = <see cref="Code"/> bên đích) và ProductCodeUser.
    /// SearchProduct.cshtml:57,121-123 hiển thị ProductCodeUser, không hiển thị ProductCode.</summary>
    public string? CodeUser { get; set; }

    /// <summary>ProductType — PRODUCT / SERVICE / COMBO.
    /// Ba giá trị này là literal trong Mst_ProductController.cs:1218-1229 (checkbox Sản phẩm/Dịch vụ/Combo).</summary>
    public string? ProductType { get; set; }

    /// <summary>BrandCode — mã nhãn hiệu (Mst_Product.cs:18 "Mã nhãn hiệu"), cột "Nhãn hiệu" SearchProduct.cshtml:69.</summary>
    public string? BrandCode { get; set; }

    /// <summary>ProductLevelSys — cấp hệ thống của mã hàng (ROOTPRD / con). SearchProduct.cshtml:106.</summary>
    public string? LevelSys { get; set; }

    /// <summary>ProductCodeBase — mã hàng cơ sở (đơn vị gốc). SearchProduct.cshtml:107.</summary>
    public string? CodeBase { get; set; }

    /// <summary>ProductCodeRoot — mã hàng gốc (Mst_Product.cs:26 "Mã hàng gốc"). SearchProduct.cshtml:108.
    /// Nguồn dùng đúng cột này làm cha của BOM: Mst_ProductController.cs:2002 ProductCodeParent = ProductCodeRoot.</summary>
    public string? CodeRoot { get; set; }

    /// <summary>FlagBuy — "Được mua". Cờ '1'/'0' (SearchProduct.cshtml:140-151 so với "1").</summary>
    public string FlagBuy { get; set; } = "1";

    /// <summary>FlagSell — "Được bán". Cờ '1'/'0' (SearchProduct.cshtml:154-162).</summary>
    public string FlagSell { get; set; } = "1";

    /// <summary>QtyEffSt — tồn tối ưu (Mst_Product.cs:40 "Tồn tối ưu").
    /// ProductIngredient.cshtml:398-403: khi thêm dòng BOM, nếu QtyEffSt null thì mặc định 1 — nó là số lượng gợi ý.</summary>
    public decimal? QtyEffSt { get; set; }

    // ----- Tab "Thông tin khác" — port 1:1 ProductOtherInformation.cshtml -----

    /// <summary>ProductNameEN — "Tên tiếng anh" (ProductOtherInformation.cshtml:15-18).</summary>
    public string? NameEn { get; set; }
    /// <summary>ProductStd — "Tiêu chuẩn" (ProductOtherInformation.cshtml:24-27).</summary>
    public string? Std { get; set; }
    /// <summary>ProductOrigin — "Xuất xứ" (ProductOtherInformation.cshtml:71-74).</summary>
    public string? Origin { get; set; }
    /// <summary>ProductExpiry — "Hạn sử dụng" (ProductOtherInformation.cshtml:80-83). Nguồn là TEXT tự do, không phải ngày.</summary>
    public string? Expiry { get; set; }
    /// <summary>ProductQuyCach — "Quy cách" (ProductOtherInformation.cshtml:93-96).</summary>
    public string? QuyCach { get; set; }
    /// <summary>ProductMnfUrl — "Link quy trình XS" (ProductOtherInformation.cshtml:102-105).</summary>
    public string? MnfUrl { get; set; }
    /// <summary>ProductIntro — tab "Chi tiết sản phẩm" (ProductOtherInformation.cshtml:126,144).</summary>
    public string? Intro { get; set; }
    /// <summary>ProductUserGuide — tab "Hướng dẫn sử dụng" (ProductOtherInformation.cshtml:129,155).</summary>
    public string? UserGuide { get; set; }
    /// <summary>ProductDrawing — tab "Bản vẽ" (ProductOtherInformation.cshtml:132,165).</summary>
    public string? Drawing { get; set; }
    /// <summary>FlagSerial — "Quản lý Serial", cờ '1'/'0' (ProductOtherInformation.cshtml:38-47).</summary>
    public string FlagSerial { get; set; } = "0";
    /// <summary>FlagLot — "Quản lý LOT", cờ '1'/'0' (ProductOtherInformation.cshtml:53-62).</summary>
    public string FlagLot { get; set; } = "0";

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

    // ----- Port 1:1 bảng Prd_BOM (ProductCenter.Common/Models/Prd_BOM.cs:9-29) -----

    /// <summary>ProductCodeParent — mã hàng CHA của dòng BOM.
    /// Nguồn gán = ProductCodeRoot của thành phẩm (Mst_ProductController.cs:2000), KHÔNG phải ProductCode.
    /// Bên đích quan hệ cha đã có qua <see cref="ProductId"/>; cột này giữ để đối chiếu dữ liệu nguồn.</summary>
    public string? ParentCodeRoot { get; set; }

    /// <summary>mp_UPBuy — giá mua của THÀNH PHẦN, đọc kèm từ Mst_Product khi lấy BOM
    /// (ProductIngredient.cshtml:107 cột "Giá mua"). Tiền tố mp_ = Mst_Product join vào.</summary>
    public decimal ComponentCostPrice { get; set; }

    /// <summary>mp_UPSell — giá bán của THÀNH PHẦN (ProductIngredient.cshtml:109 cột "Giá bán").</summary>
    public decimal ComponentSalePrice { get; set; }

    /// <summary>FlagActive — cờ hiệu lực dòng BOM, '1'/'0' (Prd_BOM.cs:17).</summary>
    public string FlagActive { get; set; } = "1";

    /// <summary>Tổng tiền mua = mp_UPBuy × Qty. Công thức nguồn ProductIngredient.cshtml:95.</summary>
    public decimal CostAmount => ComponentCostPrice * Quantity;

    /// <summary>Tổng tiền bán = mp_UPSell × Qty. Công thức nguồn ProductIngredient.cshtml:96.</summary>
    public decimal SaleAmount => ComponentSalePrice * Quantity;

    public Product Product { get; set; } = null!;
}
