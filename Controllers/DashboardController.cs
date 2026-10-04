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

        var lowStockProducts = await _context.Products
            .AsNoTracking()
            .CountAsync(product =>
                product.StockQuantity <= product.ReorderLevel);

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
            RecentTransactions = recentTransactions
        };

        return Ok(summary);
    }
}