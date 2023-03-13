using GeocodingService.DTOs;
using GeocodingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeocodingService.Controllers;

[ApiController]
[Route("[controller]")]
public class GeocodeController : ControllerBase
{
    private readonly ILogger<GeocodeController> _logger;
    private readonly IGeocodeService _geocodingService;

    public GeocodeController(ILogger<GeocodeController> logger, IGeocodeService geocodingService)
    {
        _logger = logger;
        _geocodingService = geocodingService;
    }

    [HttpGet("GetCoordinatesFromAddress")]
    public async Task<IActionResult> GetCoordinatesFromAddress([FromQuery] string address)
    {
        _logger.LogInformation("Get Coordinates From Address  at {DT}",
            DateTime.UtcNow.ToLongTimeString());

        var result = await _geocodingService.GetCoordinatesFromAddress(address);

        return Ok(result);
    }

    [HttpGet("GetAddressFromCoordinates")]
    public async Task<IActionResult> GetAddressFromCoordinates([FromQuery] double latitude, double longitude)
    {
        _logger.LogInformation("Get Address From Coordinates  at {DT}",
           DateTime.UtcNow.ToLongTimeString());

        var result = await _geocodingService.GetAddressFromCoordinates(latitude, longitude);

        return Ok(result);
    }
}