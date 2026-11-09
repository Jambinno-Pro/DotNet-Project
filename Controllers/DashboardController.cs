using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Data;
using StockFlow.Api.DTOs;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly StockFlowDbContext _context;

    public DashboardController(StockFlowDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
    {
        var totalProducts = await _context.Products
            .AsNoTracking()
            .CountAsync();

        var totalStock = await _context.Products
            .AsNoTracking()
            .SumAsync(product => product.StockQuantity);

        var totalStockValue = await _context.Products
            .AsNoTracking()
            .SumAsync(product => product.Price * product.StockQuantity);

        var totalCategories = await _context.Categories
            .AsNoTracking()
            .CountAsync();

        var totalSuppliers = await _context.Suppliers
            .AsNoTracking()
            .CountAsync();

        var totalWarehouses = await _context.Warehouses
            .AsNoTracking()
            .CountAsync();

        var totalTransactions = await _context.StockTransactions
            .AsNoTracking()
            .CountAsync();

        // Low stock products
        var lowStockItems = await _context.Products
            .AsNoTracking()
            .Where(product =>
                product.StockQuantity <= product.ReorderLevel)
            .OrderBy(product => product.StockQuantity)
            .ThenBy(product => product.Name)
            .Take(10)
            .Select(product => new DashboardProductDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                StockQuantity = product.StockQuantity,
                ReorderLevel = product.ReorderLevel,
                Price = product.Price,
                StockValue = product.Price * product.StockQuantity
            })
            .ToListAsync();

        var lowStockProducts = await _context.Products
            .AsNoTracking()
            .CountAsync(product =>
                product.StockQuantity <= product.ReorderLevel);

        // Recent transactions
        var recentTransactions = await _context.StockTransactions
            .AsNoTracking()
            .OrderByDescending(transaction => transaction.CreatedAt)
            .Take(10)
            .Select(transaction => new DashboardTransactionDto
            {
                Id = transaction.Id,
                Product = transaction.Product.Name,
                Warehouse = transaction.Warehouse.Name,
                Type = transaction.Type,
                Quantity = transaction.Quantity,
                Reference = transaction.Reference,
                CreatedAt = transaction.CreatedAt
            })
            .ToListAsync();

        // Inventory overview
        // Shows stock movement for the last 7 days.
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-6);

        var inventoryTransactions = await _context.StockTransactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.CreatedAt >= startDate &&
                transaction.CreatedAt < today.AddDays(1))
            .Select(transaction => new
            {
                transaction.CreatedAt,
                transaction.Type,
                transaction.Quantity
            })
            .ToListAsync();

        var inventoryOverview = Enumerable
            .Range(0, 7)
            .Select(offset =>
            {
                var date = startDate.AddDays(offset);

                var stockIn = inventoryTransactions
                    .Where(transaction =>
                        transaction.CreatedAt.Date == date &&
                        transaction.Type == "STOCK_IN")
                    .Sum(transaction => transaction.Quantity);

                var stockOut = inventoryTransactions
                    .Where(transaction =>
                        transaction.CreatedAt.Date == date &&
                        transaction.Type == "STOCK_OUT")
                    .Sum(transaction => transaction.Quantity);

                return new DashboardInventoryOverviewDto
                {
                    Date = date,
                    StockIn = stockIn,
                    StockOut = stockOut
                };
            })
            .ToList();

        // Stock by warehouse
        var warehouseStock = await _context.Warehouses
            .AsNoTracking()
            .Select(warehouse => new
            {
                warehouse.Id,
                warehouse.Name,
                StockQuantity = warehouse.Products
                    .Sum(product => product.StockQuantity)
            })
            .ToListAsync();

        var warehouseTotalStock = warehouseStock
            .Sum(warehouse => warehouse.StockQuantity);

        var stockByWarehouse = warehouseStock
            .Select(warehouse => new DashboardWarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                StockQuantity = warehouse.StockQuantity,
                Percentage = warehouseTotalStock > 0
                    ? Math.Round(
                        (decimal)warehouse.StockQuantity /
                        warehouseTotalStock * 100,
                        2)
                    : 0
            })
            .OrderByDescending(warehouse => warehouse.StockQuantity)
            .ToList();

        var summary = new DashboardSummaryDto
        {
            TotalProducts = totalProducts,
            TotalStock = totalStock,
            TotalStockValue = totalStockValue,
            TotalCategories = totalCategories,
            TotalSuppliers = totalSuppliers,
            TotalWarehouses = totalWarehouses,
            TotalTransactions = totalTransactions,
            LowStockProducts = lowStockProducts,
            LowStockItems = lowStockItems,
            RecentTransactions = recentTransactions,
            InventoryOverview = inventoryOverview,
            StockByWarehouse = stockByWarehouse
        };

        return Ok(summary);
    }
}