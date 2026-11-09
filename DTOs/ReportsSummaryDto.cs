namespace StockFlow.Api.DTOs;

public class ReportsSummaryDto
{
    public decimal TotalSales { get; set; }

    public int TotalOrders { get; set; }

    public int CompletedOrders { get; set; }

    public int PendingOrders { get; set; }

    public int CancelledOrders { get; set; }

    public int TotalProducts { get; set; }

    public int TotalStock { get; set; }

    public decimal InventoryValue { get; set; }

    public int LowStockProducts { get; set; }

    public int TotalStockTransactions { get; set; }
}