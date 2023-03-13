using GeocodingService.DTOs;

namespace GeocodingService.Services;

public interface IGeocodeService
{
    Task<AddressDTO> GetAddressFromCoordinates(double latitude, double longitude);
    Task<CoordinatesDTO> GetCoordinatesFromAddress(string address);

}
