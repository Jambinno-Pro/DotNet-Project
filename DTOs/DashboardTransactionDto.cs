namespace StockFlow.Api.DTOs;

public class DashboardTransactionDto
{
    public int Id { get; set; }
    public string Product { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}