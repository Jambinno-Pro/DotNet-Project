namespace StockFlow.Api.DTOs;

public class OrderDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public int? CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Notes { get; set; } = string.Empty;

    public List<OrderItemDto> Items { get; set; } = [];
}

public class OrderItemDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string Product { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}