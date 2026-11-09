namespace StockFlow.Api.Models;

public class Order
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    // Keep this temporarily for existing order data.
    public string CustomerName { get; set; } = string.Empty;

    // Customer relationship
    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "PENDING";

    public decimal TotalAmount { get; set; }

    public string Notes { get; set; } = string.Empty;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}