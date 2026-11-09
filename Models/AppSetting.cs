
namespace StockFlow.Api.Models;

public class AppSetting
{
    public int Id { get; set; }

    // Company profile
    public string CompanyName { get; set; } = "StockFlow";
    public string RegistrationNumber { get; set; } = string.Empty;

    // Business address
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Contact details
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AlternativePhone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}