using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Data;
using StockFlow.Api.DTOs;
using StockFlow.Api.Models;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly StockFlowDbContext _context;

    public CategoriesController(StockFlowDbContext context)
    {
        _context = context;
    }

    // GET: api/categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Select(category => new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                ProductCount = category.Products.Count
            })
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/categories/1
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(category => category.Id == id)
            .Select(category => new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                ProductCount = category.Products.Count
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    // POST: api/categories
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        CreateCategoryDto dto)
    {
        var name = dto.Name.Trim();

        var nameExists = await _context.Categories
            .AnyAsync(category => category.Name == name);

        if (nameExists)
        {
            return Conflict(new
            {
                message = "A category with this name already exists."
            });
        }

        var category = new Category
        {
            Name = name,
            Description = dto.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        var result = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            ProductCount = 0
        };

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.Id },
            result
        );
    }

    // PUT: api/categories/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        var name = dto.Name.Trim();

        var nameExists = await _context.Categories
            .AnyAsync(category =>
                category.Name == name &&
                category.Id != id);

        if (nameExists)
        {
            return Conflict(new
            {
                message = "A category with this name already exists."
            });
        }

        category.Name = name;
        category.Description = dto.Description.Trim();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/categories/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .Include(category => category.Products)
            .FirstOrDefaultAsync(category => category.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        if (category.Products.Any())
        {
            return Conflict(new
            {
                message = "Cannot delete a category that has products assigned to it."
            });
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}