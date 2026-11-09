namespace StockFlow.Api.DTOs;

public class DashboardInventoryOverviewDto
{
    public DateTime Date { get; set; }

    public int StockIn { get; set; }

    public int StockOut { get; set; }
}