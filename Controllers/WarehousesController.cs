using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Data;
using StockFlow.Api.DTOs;
using StockFlow.Api.Models;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly StockFlowDbContext _context;

    public WarehousesController(StockFlowDbContext context)
    {
        _context = context;
    }

    // GET: api/warehouses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetWarehouses()
    {
        var warehouses = await _context.Warehouses
            .AsNoTracking()
            .Select(warehouse => new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Code = warehouse.Code,
                Address = warehouse.Address,
                City = warehouse.City,
                Country = warehouse.Country,
                CreatedAt = warehouse.CreatedAt,
                ProductCount = warehouse.Products.Count
            })
            .ToListAsync();

        return Ok(warehouses);
    }

    // GET: api/warehouses/1
    [HttpGet("{id}")]
    public async Task<ActionResult<WarehouseDto>> GetWarehouse(int id)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .Where(warehouse => warehouse.Id == id)
            .Select(warehouse => new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Code = warehouse.Code,
                Address = warehouse.Address,
                City = warehouse.City,
                Country = warehouse.Country,
                CreatedAt = warehouse.CreatedAt,
                ProductCount = warehouse.Products.Count
            })
            .FirstOrDefaultAsync();

        if (warehouse == null)
        {
            return NotFound();
        }

        return Ok(warehouse);
    }

    // POST: api/warehouses
    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> CreateWarehouse(
        CreateWarehouseDto dto)
    {
        var name = dto.Name.Trim();
        var code = dto.Code.Trim().ToUpperInvariant();

        var nameExists = await _context.Warehouses
            .AnyAsync(warehouse => warehouse.Name == name);

        if (nameExists)
        {
            return Conflict(new
            {
                message = "A warehouse with this name already exists."
            });
        }

        var codeExists = await _context.Warehouses
            .AnyAsync(warehouse => warehouse.Code == code);

        if (codeExists)
        {
            return Conflict(new
            {
                message = "A warehouse with this code already exists."
            });
        }

        var warehouse = new Warehouse
        {
            Name = name,
            Code = code,
            Address = dto.Address.Trim(),
            City = dto.City.Trim(),
            Country = dto.Country.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Warehouses.Add(warehouse);

        await _context.SaveChangesAsync();

        var result = new WarehouseDto
        {
            Id = warehouse.Id,
            Name = warehouse.Name,
            Code = warehouse.Code,
            Address = warehouse.Address,
            City = warehouse.City,
            Country = warehouse.Country,
            CreatedAt = warehouse.CreatedAt,
            ProductCount = 0
        };

        return CreatedAtAction(
            nameof(GetWarehouse),
            new { id = warehouse.Id },
            result
        );
    }

    // PUT: api/warehouses/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWarehouse(
        int id,
        UpdateWarehouseDto dto)
    {
        var warehouse = await _context.Warehouses.FindAsync(id);

        if (warehouse == null)
        {
            return NotFound();
        }

        var name = dto.Name.Trim();
        var code = dto.Code.Trim().ToUpperInvariant();

        var nameExists = await _context.Warehouses
            .AnyAsync(w =>
                w.Name == name &&
                w.Id != id);

        if (nameExists)
        {
            return Conflict(new
            {
                message = "A warehouse with this name already exists."
            });
        }

        var codeExists = await _context.Warehouses
            .AnyAsync(w =>
                w.Code == code &&
                w.Id != id);

        if (codeExists)
        {
            return Conflict(new
            {
                message = "A warehouse with this code already exists."
            });
        }

        warehouse.Name = name;
        warehouse.Code = code;
        warehouse.Address = dto.Address.Trim();
        warehouse.City = dto.City.Trim();
        warehouse.Country = dto.Country.Trim();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/warehouses/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWarehouse(int id)
    {
        var warehouse = await _context.Warehouses
            .Include(warehouse => warehouse.Products)
            .FirstOrDefaultAsync(warehouse => warehouse.Id == id);

        if (warehouse == null)
        {
            return NotFound();
        }

        if (warehouse.Products.Any())
        {
            return Conflict(new
            {
                message = "Cannot delete a warehouse that has products assigned to it."
            });
        }

        _context.Warehouses.Remove(warehouse);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}