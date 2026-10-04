using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Data;
using StockFlow.Api.DTOs;
using StockFlow.Api.Models;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly StockFlowDbContext _context;

    public ProductsController(StockFlowDbContext context)
    {
        _context = context;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? supplierId,
        [FromQuery] int? warehouseId,
        [FromQuery] bool? lowStock,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortOrder = "asc")
    {
        if (page < 1)
        {
            return BadRequest(new
            {
                message = "Page must be greater than 0."
            });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                message = "Page size must be between 1 and 100."
            });
        }

        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.CategoryNavigation)
            .Include(p => p.SupplierNavigation)
            .Include(p => p.WarehouseNavigation)
            .AsQueryable();

        // Search by product name or SKU
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(p =>
                p.Name.Contains(term) ||
                p.SKU.Contains(term));
        }

        // Filter by category
        if (categoryId.HasValue)
        {
            query = query.Where(p =>
                p.CategoryId == categoryId.Value);
        }

        // Filter by supplier
        if (supplierId.HasValue)
        {
            query = query.Where(p =>
                p.SupplierId == supplierId.Value);
        }

        // Filter by warehouse
        if (warehouseId.HasValue)
        {
            query = query.Where(p =>
                p.WarehouseId == warehouseId.Value);
        }

        // Filter low-stock products
        if (lowStock == true)
        {
            query = query.Where(p =>
                p.StockQuantity <= p.ReorderLevel);
        }

        // Validate sorting
        var allowedSortFields = new[]
        {
            "name",
            "price",
            "stockquantity",
            "createdat"
        };

        sortBy = sortBy.Trim().ToLowerInvariant();
        sortOrder = sortOrder.Trim().ToLowerInvariant();

        if (!allowedSortFields.Contains(sortBy))
        {
            return BadRequest(new
            {
                message = "Invalid sortBy value. Allowed values: name, price, stockQuantity, createdAt."
            });
        }

        if (sortOrder != "asc" && sortOrder != "desc")
        {
            return BadRequest(new
            {
                message = "Invalid sortOrder value. Allowed values: asc, desc."
            });
        }

        // Count filtered products before pagination
        var totalItems = await query.CountAsync();

        var totalPages = (int)Math.Ceiling(
            totalItems / (double)pageSize);

        // Apply sorting
        query = sortBy switch
        {
            "price" => sortOrder == "asc"
                ? query.OrderBy(p => p.Price)
                : query.OrderByDescending(p => p.Price),

            "stockquantity" => sortOrder == "asc"
                ? query.OrderBy(p => p.StockQuantity)
                : query.OrderByDescending(p => p.StockQuantity),

            "createdat" => sortOrder == "asc"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),

            _ => sortOrder == "asc"
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name)
        };

        // Apply pagination
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,

                CategoryId = p.CategoryId,
                Category = p.CategoryNavigation!.Name,

                SupplierId = p.SupplierId,
                Supplier = p.SupplierNavigation != null
                    ? p.SupplierNavigation.Name
                    : string.Empty,

                WarehouseId = p.WarehouseId,
                Warehouse = p.WarehouseNavigation != null
                    ? p.WarehouseNavigation.Name
                    : string.Empty,

                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ReorderLevel = p.ReorderLevel,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        var result = new PagedResultDto<ProductDto>
        {
            Items = products,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return Ok(result);
    }

    // GET: api/products/1
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.CategoryNavigation)
            .Include(p => p.SupplierNavigation)
            .Include(p => p.WarehouseNavigation)
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,

                CategoryId = p.CategoryId,
                Category = p.CategoryNavigation!.Name,

                SupplierId = p.SupplierId,
                Supplier = p.SupplierNavigation != null
                    ? p.SupplierNavigation.Name
                    : string.Empty,

                WarehouseId = p.WarehouseId,
                Warehouse = p.WarehouseNavigation != null
                    ? p.WarehouseNavigation.Name
                    : string.Empty,

                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ReorderLevel = p.ReorderLevel,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        CreateProductDto dto)
    {
        var name = dto.Name.Trim();
        var sku = dto.SKU.Trim().ToUpperInvariant();

        // Check duplicate SKU
        if (await _context.Products.AnyAsync(p => p.SKU == sku))
        {
            return Conflict(new
            {
                message = "A product with this SKU already exists."
            });
        }

        // Check category
        if (!await _context.Categories.AnyAsync(c =>
            c.Id == dto.CategoryId))
        {
            return BadRequest(new
            {
                message = "The specified category does not exist."
            });
        }

        // Check supplier
        if (dto.SupplierId.HasValue &&
            !await _context.Suppliers.AnyAsync(s =>
                s.Id == dto.SupplierId.Value))
        {
            return BadRequest(new
            {
                message = "The specified supplier does not exist."
            });
        }

        // Check warehouse
        if (dto.WarehouseId.HasValue &&
            !await _context.Warehouses.AnyAsync(w =>
                w.Id == dto.WarehouseId.Value))
        {
            return BadRequest(new
            {
                message = "The specified warehouse does not exist."
            });
        }

        var product = new Product
        {
            Name = name,
            SKU = sku,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId,
            WarehouseId = dto.WarehouseId,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            ReorderLevel = dto.ReorderLevel,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        // Load related data for response
        var result = await _context.Products
            .AsNoTracking()
            .Include(p => p.CategoryNavigation)
            .Include(p => p.SupplierNavigation)
            .Include(p => p.WarehouseNavigation)
            .Where(p => p.Id == product.Id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,

                CategoryId = p.CategoryId,
                Category = p.CategoryNavigation!.Name,

                SupplierId = p.SupplierId,
                Supplier = p.SupplierNavigation != null
                    ? p.SupplierNavigation.Name
                    : string.Empty,

                WarehouseId = p.WarehouseId,
                Warehouse = p.WarehouseNavigation != null
                    ? p.WarehouseNavigation.Name
                    : string.Empty,

                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ReorderLevel = p.ReorderLevel,
                CreatedAt = p.CreatedAt
            })
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            result);
    }

    // PUT: api/products/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        var name = dto.Name.Trim();
        var sku = dto.SKU.Trim().ToUpperInvariant();

        // Check duplicate SKU
        if (await _context.Products.AnyAsync(p =>
            p.SKU == sku && p.Id != id))
        {
            return Conflict(new
            {
                message = "A product with this SKU already exists."
            });
        }

        // Check category
        if (!await _context.Categories.AnyAsync(c =>
            c.Id == dto.CategoryId))
        {
            return BadRequest(new
            {
                message = "The specified category does not exist."
            });
        }

        // Check supplier
        if (dto.SupplierId.HasValue &&
            !await _context.Suppliers.AnyAsync(s =>
                s.Id == dto.SupplierId.Value))
        {
            return BadRequest(new
            {
                message = "The specified supplier does not exist."
            });
        }

        // Check warehouse
        if (dto.WarehouseId.HasValue &&
            !await _context.Warehouses.AnyAsync(w =>
                w.Id == dto.WarehouseId.Value))
        {
            return BadRequest(new
            {
                message = "The specified warehouse does not exist."
            });
        }

        product.Name = name;
        product.SKU = sku;
        product.CategoryId = dto.CategoryId;
        product.SupplierId = dto.SupplierId;
        product.WarehouseId = dto.WarehouseId;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.ReorderLevel = dto.ReorderLevel;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/products/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        // Prevent deleting products with transaction history
        var hasTransactions = await _context.StockTransactions
            .AnyAsync(t => t.ProductId == id);

        if (hasTransactions)
        {
            return Conflict(new
            {
                message = "Cannot delete a product that has stock transactions."
            });
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}