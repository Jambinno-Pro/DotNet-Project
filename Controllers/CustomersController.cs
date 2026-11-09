using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Data;
using StockFlow.Api.DTOs;
using StockFlow.Api.Models;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly StockFlowDbContext _context;

    public CustomersController(StockFlowDbContext context)
    {
        _context = context;
    }

    // GET: api/customers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .Select(customer => new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                Country = customer.Country,
                CreatedAt = customer.CreatedAt,
                OrderCount = customer.Orders.Count
            })
            .OrderBy(customer => customer.Name)
            .ToListAsync();

        return Ok(customers);
    }

    // GET: api/customers/1
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Where(customer => customer.Id == id)
            .Select(customer => new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                Country = customer.Country,
                CreatedAt = customer.CreatedAt,
                OrderCount = customer.Orders.Count
            })
            .FirstOrDefaultAsync();

        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    // POST: api/customers
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(
        CreateCustomerDto dto)
    {
        var name = dto.Name.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();

        var emailExists = await _context.Customers
            .AnyAsync(customer => customer.Email == email);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "A customer with this email already exists."
            });
        }

        var customer = new Customer
        {
            Name = name,
            Email = email,
            Phone = dto.Phone.Trim(),
            Address = dto.Address.Trim(),
            City = dto.City.Trim(),
            Country = dto.Country.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);

        await _context.SaveChangesAsync();

        var result = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            City = customer.City,
            Country = customer.Country,
            CreatedAt = customer.CreatedAt,
            OrderCount = 0
        };

        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = customer.Id },
            result
        );
    }

    // PUT: api/customers/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(
        int id,
        UpdateCustomerDto dto)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(customer => customer.Id == id);

        if (customer == null)
            return NotFound();

        var name = dto.Name.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();

        var emailExists = await _context.Customers
            .AnyAsync(customer =>
                customer.Email == email &&
                customer.Id != id);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "A customer with this email already exists."
            });
        }

        customer.Name = name;
        customer.Email = email;
        customer.Phone = dto.Phone.Trim();
        customer.Address = dto.Address.Trim();
        customer.City = dto.City.Trim();
        customer.Country = dto.Country.Trim();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/customers/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers
            .Include(customer => customer.Orders)
            .FirstOrDefaultAsync(customer => customer.Id == id);

        if (customer == null)
            return NotFound();

        if (customer.Orders.Any())
        {
            return Conflict(new
            {
                message = "Cannot delete a customer that has orders."
            });
        }

        _context.Customers.Remove(customer);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}