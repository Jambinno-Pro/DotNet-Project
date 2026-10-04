using System.ComponentModel.DataAnnotations;

namespace StockFlow.Api.DTOs;

public class CreateStockTransactionDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [StringLength(100)]
    public string Reference { get; set; } = string.Empty;

    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
}