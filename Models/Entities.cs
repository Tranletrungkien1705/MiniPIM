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
