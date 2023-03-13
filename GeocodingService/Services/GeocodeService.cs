using GeocodingService.DTOs;
using GeocodingService.Exceptions;
using GeocodingService.Models;
using GeocodingService.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text.Json;

namespace GeocodingService.Services;

public class GeocodeService : IGeocodeService
{
    private readonly HttpClient _httpClient;
    private readonly IMongoCollection<CacheModel> _cacheCollection;
    private readonly ILogger<GeocodeService> _logger;

    private readonly string apiKey = "AIzaSyA37G_uPYDtGK-2qEd0rdonddWbBzgJAQw";

    public GeocodeService(ILogger<GeocodeService> logger, IOptions<CacheStoreSettings> cacheSettings)
    {
        _logger = logger;
        _httpClient = new HttpClient();

        var mongoClient = new MongoClient(
          cacheSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            cacheSettings.Value.DatabaseName);

        _cacheCollection = mongoDatabase.GetCollection<CacheModel>(
            cacheSettings.Value.CollectionName);
    }

    public async Task<AddressDTO> GetAddressFromCoordinates(double latitude, double longitude)
    {
         string cacheKey = $"{latitude}|{longitude}";

        var resultFromCache = (await _cacheCollection.FindAsync(x => x.Coordinates == cacheKey)).FirstOrDefault();

        if (resultFromCache is not null)
        {
            return new AddressDTO
            {
                Address = resultFromCache.Address
            };
        }

        string url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude.ToString().Replace(',','.')},{longitude.ToString().Replace(',', '.')}&key={apiKey}";

        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve address from coordinates at {DT} from Google Api",
             DateTime.UtcNow.ToLongTimeString());

            throw new GoogleApiException("Failed to retrieve address from coordinates");
        }
      
        string json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<RootDTO>(json);

        if (data.results.Count == 0)
        {
            _logger.LogError("Not found address from coordinates at {DT} from Google Api",
            DateTime.UtcNow.ToLongTimeString());

            throw new NotFoundException("Not found address from coordinates");
        }

        string address = data.results[0].formatted_address;

        await _cacheCollection.InsertOneAsync(new CacheModel 
        { 
            Address = address,
            Coordinates = cacheKey 
        });

        return new AddressDTO
        {
            Address = address
        };
    }

    public async Task<CoordinatesDTO> GetCoordinatesFromAddress(string address)
    {
        var resultFromCache = (await _cacheCollection.FindAsync(x => x.Address == address)).FirstOrDefault();

        if (resultFromCache is not null)
        {
            return new CoordinatesDTO
            {
                Latitude = resultFromCache.Coordinates.Split('|')[0],
                Longitude = resultFromCache.Coordinates.Split('|')[1]
            };
        }

        string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={apiKey}";

        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve coordinates from address at {DT} from Google Api",
            DateTime.UtcNow.ToLongTimeString());

            throw new GoogleApiException("Failed to retrieve coordinates from address");
        }

        string json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<RootDTO>(json);

        if(data.results.Count == 0)
        {
            _logger.LogError("Not found coordinates from address at {DT} from Google Api",
            DateTime.UtcNow.ToLongTimeString());

            throw new NotFoundException("Not found coordinates from address");
        }

        double lat = data.results[0].geometry.location.lat;
        double lng = data.results[0].geometry.location.lng;

        await _cacheCollection.InsertOneAsync(new CacheModel
        {
            Address = address,
            Coordinates = $"{lat}|{lng}"
        });

        return new CoordinatesDTO
        {
            Latitude = lat.ToString(),
            Longitude = lng.ToString()
        }; 
    }
}
