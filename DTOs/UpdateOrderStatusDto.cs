using System.ComponentModel.DataAnnotations;

namespace StockFlow.Api.DTOs;

public class UpdateOrderStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}