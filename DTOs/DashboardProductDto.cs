namespace StockFlow.Api.DTOs;

public class DashboardProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public decimal Price { get; set; }
    public decimal StockValue { get; set; }
}