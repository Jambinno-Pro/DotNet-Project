using System.ComponentModel.DataAnnotations;

namespace StockFlow.Api.DTOs;

public class UpdateCustomerDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
}