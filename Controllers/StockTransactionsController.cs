using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Data;
using StockFlow.Api.DTOs;
using StockFlow.Api.Models;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockTransactionsController : ControllerBase
{
    private readonly StockFlowDbContext _context;

    public StockTransactionsController(StockFlowDbContext context)
    {
        _context = context;
    }

    // GET: api/stocktransactions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockTransactionDto>>> GetTransactions()
    {
        var transactions = await _context.StockTransactions
            .AsNoTracking()
            .Include(t => t.Product)
            .Include(t => t.Warehouse)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new StockTransactionDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                Product = t.Product.Name,
                WarehouseId = t.WarehouseId,
                Warehouse = t.Warehouse.Name,
                Type = t.Type,
                Quantity = t.Quantity,
                Reference = t.Reference,
                Notes = t.Notes,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return Ok(transactions);
    }

    // GET: api/stocktransactions/1
    [HttpGet("{id}")]
    public async Task<ActionResult<StockTransactionDto>> GetTransaction(int id)
    {
        var transaction = await _context.StockTransactions
            .AsNoTracking()
            .Include(t => t.Product)
            .Include(t => t.Warehouse)
            .Where(t => t.Id == id)
            .Select(t => new StockTransactionDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                Product = t.Product.Name,
                WarehouseId = t.WarehouseId,
                Warehouse = t.Warehouse.Name,
                Type = t.Type,
                Quantity = t.Quantity,
                Reference = t.Reference,
                Notes = t.Notes,
                CreatedAt = t.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (transaction == null)
            return NotFound();

        return Ok(transaction);
    }

    // POST: api/stocktransactions
    [HttpPost]
    public async Task<ActionResult<StockTransactionDto>> CreateTransaction(
        CreateStockTransactionDto dto)
    {
        var type = dto.Type.Trim().ToUpperInvariant();
        var reference = dto.Reference.Trim();
        var notes = dto.Notes.Trim();

        var validTypes = new[] { "STOCK_IN", "STOCK_OUT", "ADJUSTMENT" };

        if (!validTypes.Contains(type))
        {
            return BadRequest(new
            {
                message = "Type must be STOCK_IN, STOCK_OUT, or ADJUSTMENT."
            });
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

        if (product == null)
        {
            return BadRequest(new
            {
                message = "The specified product does not exist."
            });
        }

        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == dto.WarehouseId);

        if (warehouse == null)
        {
            return BadRequest(new
            {
                message = "The specified warehouse does not exist."
            });
        }

        if (product.WarehouseId != dto.WarehouseId)
        {
            return BadRequest(new
            {
                message = "The specified warehouse is not assigned to this product."
            });
        }

        if (type == "STOCK_OUT" &&
            product.StockQuantity < dto.Quantity)
        {
            return BadRequest(new
            {
                message = "Insufficient stock for this transaction."
            });
        }

        switch (type)
        {
            case "STOCK_IN":
                product.StockQuantity += dto.Quantity;
                break;

            case "STOCK_OUT":
                product.StockQuantity -= dto.Quantity;
                break;

            case "ADJUSTMENT":
                product.StockQuantity = dto.Quantity;
                break;
        }

        var transaction = new StockTransaction
        {
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            Type = type,
            Quantity = dto.Quantity,
            Reference = reference,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.StockTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        var result = new StockTransactionDto
        {
            Id = transaction.Id,
            ProductId = product.Id,
            Product = product.Name,
            WarehouseId = warehouse.Id,
            Warehouse = warehouse.Name,
            Type = transaction.Type,
            Quantity = transaction.Quantity,
            Reference = transaction.Reference,
            Notes = transaction.Notes,
            CreatedAt = transaction.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetTransaction),
            new { id = transaction.Id },
            result
        );
    }
}