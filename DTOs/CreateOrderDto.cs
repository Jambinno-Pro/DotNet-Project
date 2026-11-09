using System.ComponentModel.DataAnnotations;

namespace StockFlow.Api.DTOs;

public class CreateOrderDto
{
    [Range(1, int.MaxValue)]
    public int? CustomerId { get; set; }

    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = [];

    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
}

public class CreateOrderItemDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}