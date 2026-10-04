namespace StockFlow.Api.DTOs;

public class DashboardSummaryDto
{
    public int TotalProducts { get; set; }

    public int TotalStock { get; set; }

    public decimal TotalStockValue { get; set; }

    public int TotalCategories { get; set; }

    public int TotalSuppliers { get; set; }

    public int TotalWarehouses { get; set; }

    public int TotalTransactions { get; set; }

    public int LowStockProducts { get; set; }

    public IEnumerable<DashboardProductDto> LowStockItems { get; set; }
        = [];

    public IEnumerable<DashboardTransactionDto> RecentTransactions { get; set; }
        = [];
}