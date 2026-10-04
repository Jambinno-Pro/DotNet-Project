using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Data;
using StockFlow.Api.DTOs;
using StockFlow.Api.Models;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly StockFlowDbContext _context;

    public SuppliersController(StockFlowDbContext context)
    {
        _context = context;
    }

    // GET: api/suppliers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers()
    {
        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .Select(supplier => new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                City = supplier.City,
                Country = supplier.Country,
                CreatedAt = supplier.CreatedAt,
                ProductCount = supplier.Products.Count
            })
            .ToListAsync();

        return Ok(suppliers);
    }

    // GET: api/suppliers/1
    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDto>> GetSupplier(int id)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .Where(supplier => supplier.Id == id)
            .Select(supplier => new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                City = supplier.City,
                Country = supplier.Country,
                CreatedAt = supplier.CreatedAt,
                ProductCount = supplier.Products.Count
            })
            .FirstOrDefaultAsync();

        if (supplier == null)
        {
            return NotFound();
        }

        return Ok(supplier);
    }

    // POST: api/suppliers
    [HttpPost]
    public async Task<ActionResult<SupplierDto>> CreateSupplier(
        CreateSupplierDto dto)
    {
        var name = dto.Name.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();

        var nameExists = await _context.Suppliers
            .AnyAsync(supplier => supplier.Name == name);

        if (nameExists)
        {
            return Conflict(new
            {
                message = "A supplier with this name already exists."
            });
        }

        var supplier = new Supplier
        {
            Name = name,
            ContactPerson = dto.ContactPerson.Trim(),
            Email = email,
            Phone = dto.Phone.Trim(),
            Address = dto.Address.Trim(),
            City = dto.City.Trim(),
            Country = dto.Country.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);

        await _context.SaveChangesAsync();

        var result = new SupplierDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            City = supplier.City,
            Country = supplier.Country,
            CreatedAt = supplier.CreatedAt,
            ProductCount = 0
        };

        return CreatedAtAction(
            nameof(GetSupplier),
            new { id = supplier.Id },
            result
        );
    }

    // PUT: api/suppliers/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(
        int id,
        UpdateSupplierDto dto)
    {
        var supplier = await _context.Suppliers.FindAsync(id);

        if (supplier == null)
        {
            return NotFound();
        }

        var name = dto.Name.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();

        var nameExists = await _context.Suppliers
            .AnyAsync(s =>
                s.Name == name &&
                s.Id != id);

        if (nameExists)
        {
            return Conflict(new
            {
                message = "A supplier with this name already exists."
            });
        }

        supplier.Name = name;
        supplier.ContactPerson = dto.ContactPerson.Trim();
        supplier.Email = email;
        supplier.Phone = dto.Phone.Trim();
        supplier.Address = dto.Address.Trim();
        supplier.City = dto.City.Trim();
        supplier.Country = dto.Country.Trim();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/suppliers/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var supplier = await _context.Suppliers
            .Include(supplier => supplier.Products)
            .FirstOrDefaultAsync(supplier => supplier.Id == id);

        if (supplier == null)
        {
            return NotFound();
        }

        if (supplier.Products.Any())
        {
            return Conflict(new
            {
                message = "Cannot delete a supplier that has products assigned to it."
            });
        }

        _context.Suppliers.Remove(supplier);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}