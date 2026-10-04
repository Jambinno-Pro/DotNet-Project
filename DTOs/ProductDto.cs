namespace StockFlow.Api.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;

    public int? SupplierId { get; set; }
    public string Supplier { get; set; } = string.Empty;

    public int? WarehouseId { get; set; }
    public string Warehouse { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }

    public DateTime CreatedAt { get; set; }
}