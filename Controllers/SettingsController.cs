
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Data;
using StockFlow.Api.Models;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly StockFlowDbContext _context;

    public SettingsController(StockFlowDbContext context)
    {
        _context = context;
    }

    // GET: api/settings
    [HttpGet]
    public async Task<ActionResult<AppSetting>> GetSettings()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new AppSetting();
            _context.AppSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return Ok(settings);
    }

    // PUT: api/settings
    [HttpPut]
    public async Task<ActionResult<AppSetting>> UpdateSettings(
        [FromBody] AppSetting updatedSettings)
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new AppSetting();
            _context.AppSettings.Add(settings);
        }

        // Company profile
        settings.CompanyName =
            (updatedSettings.CompanyName ?? string.Empty).Trim();

        settings.RegistrationNumber =
            (updatedSettings.RegistrationNumber ?? string.Empty).Trim();

        // Business address
        settings.Address =
            (updatedSettings.Address ?? string.Empty).Trim();

        settings.City =
            (updatedSettings.City ?? string.Empty).Trim();

        settings.Province =
            (updatedSettings.Province ?? string.Empty).Trim();

        settings.PostalCode =
            (updatedSettings.PostalCode ?? string.Empty).Trim();

        settings.Country =
            (updatedSettings.Country ?? string.Empty).Trim();

        // Contact details
        settings.Email =
            (updatedSettings.Email ?? string.Empty).Trim();

        settings.Phone =
            (updatedSettings.Phone ?? string.Empty).Trim();

        settings.AlternativePhone =
            (updatedSettings.AlternativePhone ?? string.Empty).Trim();

        settings.Website =
            (updatedSettings.Website ?? string.Empty).Trim();

        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(settings);
    }
}