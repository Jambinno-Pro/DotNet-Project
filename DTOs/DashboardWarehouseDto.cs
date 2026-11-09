namespace StockFlow.Api.DTOs;

public class DashboardWarehouseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int StockQuantity { get; set; }

    public decimal Percentage { get; set; }
}