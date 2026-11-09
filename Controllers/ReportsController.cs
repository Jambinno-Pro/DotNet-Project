using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Data;
using StockFlow.Api.DTOs;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly StockFlowDbContext _context;

    public ReportsController(StockFlowDbContext context)
    {
        _context = context;
    }

    // GET: api/reports/summary
    [HttpGet("summary")]
    public async Task<ActionResult<ReportsSummaryDto>> GetSummary()
    {
        var totalSales = await _context.Orders
            .Where(order => order.Status == "COMPLETED")
            .SumAsync(order => (decimal?)order.TotalAmount) ?? 0;

        var totalOrders = await _context.Orders.CountAsync();

        var completedOrders = await _context.Orders
            .CountAsync(order => order.Status == "COMPLETED");

        var pendingOrders = await _context.Orders
            .CountAsync(order => order.Status == "PENDING");

        var cancelledOrders = await _context.Orders
            .CountAsync(order => order.Status == "CANCELLED");

        var totalProducts = await _context.Products.CountAsync();

        var totalStock = await _context.Products
            .SumAsync(product => product.StockQuantity);

        var inventoryValue = await _context.Products
            .SumAsync(product =>
                product.Price * product.StockQuantity);

        var lowStockProducts = await _context.Products
            .CountAsync(product =>
                product.StockQuantity <= product.ReorderLevel);

        var totalStockTransactions = await _context.StockTransactions
            .CountAsync();

        var result = new ReportsSummaryDto
        {
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            CompletedOrders = completedOrders,
            PendingOrders = pendingOrders,
            CancelledOrders = cancelledOrders,
            TotalProducts = totalProducts,
            TotalStock = totalStock,
            InventoryValue = inventoryValue,
            LowStockProducts = lowStockProducts,
            TotalStockTransactions = totalStockTransactions
        };

        return Ok(result);
    }
}