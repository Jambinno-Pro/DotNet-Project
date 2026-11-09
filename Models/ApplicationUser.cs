
using Microsoft.AspNetCore.Identity;

namespace StockFlow.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Staff";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
