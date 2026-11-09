using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Data;
using StockFlow.Api.DTOs;
using StockFlow.Api.Models;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly StockFlowDbContext _context;

    public OrdersController(StockFlowDbContext context)
    {
        _context = context;
    }

    // GET: api/orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.Items)
                .ThenInclude(item => item.Product)
            .OrderByDescending(order => order.OrderDate)
            .Select(order => new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,

                CustomerName = order.Customer != null
                    ? order.Customer.Name
                    : order.CustomerName,

                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Notes = order.Notes,

                Items = order.Items
                    .Select(item => new OrderItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        Product = item.Product.Name,
                        SKU = item.Product.SKU,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }

    // GET: api/orders/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.Items)
                .ThenInclude(item => item.Product)
            .Where(order => order.Id == id)
            .Select(order => new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,

                CustomerName = order.Customer != null
                    ? order.Customer.Name
                    : order.CustomerName,

                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Notes = order.Notes,

                Items = order.Items
                    .Select(item => new OrderItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        Product = item.Product.Name,
                        SKU = item.Product.SKU,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null)
            return NotFound();

        return Ok(order);
    }

    // POST: api/orders
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        CreateOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
        {
            return BadRequest(new
            {
                message = "An order must contain at least one product."
            });
        }

        var notes = dto.Notes.Trim();

        Customer? customer = null;

        // Use CustomerId when supplied.
        if (dto.CustomerId.HasValue)
        {
            customer = await _context.Customers
                .FirstOrDefaultAsync(
                    customer => customer.Id == dto.CustomerId.Value);

            if (customer == null)
            {
                return BadRequest(new
                {
                    message = "The specified customer does not exist."
                });
            }
        }

        // Keep backward compatibility for orders that still use CustomerName.
        var customerName = dto.CustomerName.Trim();

        if (customer == null && string.IsNullOrWhiteSpace(customerName))
        {
            return BadRequest(new
            {
                message = "CustomerId or customer name is required."
            });
        }

        // If a customer is selected, always use the customer's current name.
        if (customer != null)
        {
            customerName = customer.Name;
        }

        // Prevent duplicate products in the same order.
        var duplicateProductIds = dto.Items
            .GroupBy(item => item.ProductId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateProductIds.Count > 0)
        {
            return BadRequest(new
            {
                message = "A product cannot appear more than once in an order."
            });
        }

        var productIds = dto.Items
            .Select(item => item.ProductId)
            .ToList();

        var products = await _context.Products
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
        {
            return BadRequest(new
            {
                message = "One or more specified products do not exist."
            });
        }

        var order = new Order
        {
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}",

            CustomerId = customer?.Id,
            Customer = customer,

            CustomerName = customerName,

            OrderDate = DateTime.UtcNow,

            Status = "PENDING",

            Notes = notes
        };

        decimal totalAmount = 0;

        foreach (var itemDto in dto.Items)
        {
            var product = products
                .First(product => product.Id == itemDto.ProductId);

            var lineTotal = product.Price * itemDto.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price,
                LineTotal = lineTotal
            };

            order.Items.Add(orderItem);

            totalAmount += lineTotal;
        }

        order.TotalAmount = totalAmount;

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();

        // Reload the saved order with its relationships.
        var result = await _context.Orders
            .AsNoTracking()
            .Include(savedOrder => savedOrder.Customer)
            .Include(savedOrder => savedOrder.Items)
                .ThenInclude(item => item.Product)
            .Where(savedOrder => savedOrder.Id == order.Id)
            .Select(savedOrder => new OrderDto
            {
                Id = savedOrder.Id,
                OrderNumber = savedOrder.OrderNumber,
                CustomerId = savedOrder.CustomerId,

                CustomerName = savedOrder.Customer != null
                    ? savedOrder.Customer.Name
                    : savedOrder.CustomerName,

                OrderDate = savedOrder.OrderDate,
                Status = savedOrder.Status,
                TotalAmount = savedOrder.TotalAmount,
                Notes = savedOrder.Notes,

                Items = savedOrder.Items
                    .Select(item => new OrderItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        Product = item.Product.Name,
                        SKU = item.Product.SKU,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal
                    })
                    .ToList()
            })
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.Id },
            result
        );
    }

    // PUT: api/orders/{id}/status
[HttpPut("{id}/status")]
public async Task<IActionResult> UpdateOrderStatus(
    int id,
    UpdateOrderStatusDto dto)
{
    var order = await _context.Orders
        .Include(order => order.Items)
        .ThenInclude(item => item.Product)
        .FirstOrDefaultAsync(order => order.Id == id);

    if (order == null)
        return NotFound();

    var status = dto.Status.Trim().ToUpperInvariant();

    var validStatuses = new[]
    {
        "PENDING",
        "CONFIRMED",
        "PROCESSING",
        "COMPLETED",
        "CANCELLED"
    };

    if (!validStatuses.Contains(status))
    {
        return BadRequest(new
        {
            message =
                "Status must be PENDING, CONFIRMED, PROCESSING, COMPLETED, or CANCELLED."
        });
    }

    // Prevent processing stock twice.
    if (order.Status == "COMPLETED" && status == "COMPLETED")
    {
        return BadRequest(new
        {
            message = "This order has already been completed and stock has already been deducted."
        });
    }

    // When completing an order, validate stock first.
    if (status == "COMPLETED")
    {
        foreach (var item in order.Items)
        {
            if (item.Product.StockQuantity < item.Quantity)
            {
                return BadRequest(new
                {
                    message =
                        $"Insufficient stock for product '{item.Product.Name}'. " +
                        $"Available: {item.Product.StockQuantity}, " +
                        $"Required: {item.Quantity}."
                });
            }
        }

        // Deduct stock and create STOCK_OUT transactions.
        foreach (var item in order.Items)
        {
            item.Product.StockQuantity -= item.Quantity;

            var transaction = new StockTransaction
            {
                ProductId = item.ProductId,
                WarehouseId = item.Product.WarehouseId!.Value,
                Type = "STOCK_OUT",
                Quantity = item.Quantity,
                Reference = order.OrderNumber,
                Notes = $"Stock deducted for completed order {order.OrderNumber}",
                CreatedAt = DateTime.UtcNow
            };

            _context.StockTransactions.Add(transaction);
        }
    }

    order.Status = status;

    await _context.SaveChangesAsync();

    return NoContent();
  }
}