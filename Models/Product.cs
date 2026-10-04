namespace StockFlow.Api.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category? CategoryNavigation { get; set; }

    public int? SupplierId { get; set; }
    public Supplier? SupplierNavigation { get; set; }

    public int? WarehouseId { get; set; }
    public Warehouse? WarehouseNavigation { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int ReorderLevel { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}