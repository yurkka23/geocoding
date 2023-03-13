using GeocodingService.DTOs;
using GeocodingService.Exceptions;
using GeocodingService.Services;
using GeocodingService.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;


namespace Tests.GeocodingService;

public class TestsGeocoding
{
    private readonly IOptions<CacheStoreSettings> _options;
    private readonly ILogger<GeocodeService> logger;
    public TestsGeocoding()
    {
        CacheStoreSettings mongoSettings = new CacheStoreSettings()
        {
            CollectionName = "Cache",
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "ProxyCache"
        };

        _options = Options.Create(mongoSettings);

        var mock = new Mock<ILogger<GeocodeService>>();
        logger = mock.Object;
    }

    [Fact]
    public async void GetCoordinatesFromAddressTest_SuccessOnMockData()
    {
        //Arrange
        var service = new GeocodeService(logger, _options);

        string adrress = "lviv";
        string expectedLatitude = "49,839683";
        string expectedLongitude = "24,029717";

        //Act
        var result = await service.GetCoordinatesFromAddress(adrress);

        //Asset
        result.ShouldBeOfType<CoordinatesDTO>();
        result.Latitude.ShouldBe(expectedLatitude);
        result.Longitude.ShouldBe(expectedLongitude);
    }

    [Fact]
    public async void GetCoordinatesFromAddressTest_FailOnWrongData()
    {
        //Arrange
        var service = new GeocodeService(logger, _options);

        string adrress = "ffftttttt";
    
        //Act

        //Asset
        await Assert.ThrowsAsync<NotFoundException>(async () => await service.GetCoordinatesFromAddress(adrress));
    }

    [Fact]
    public async void GetAddressFromCoordinatesTest_SuccessOnMockData()
    {
        //Arrange
        var service = new GeocodeService(logger, _options);

        string adrress = "lviv";
        double expectedLatitude = 49.839683;
        double expectedLongitude = 24.029717;

        //Act
        var result = await service.GetAddressFromCoordinates(expectedLatitude ,expectedLongitude);

        //Asset
        result.ShouldBeOfType<AddressDTO>();
        result.Address.ShouldBe(adrress);
    }

    [Fact]
    public async void GetAddressFromCoordinatesTest_FailOnWrongData()
    {
        //Arrange
        var service = new GeocodeService(logger, _options);

        double wrongLatitude = 200;
        double wrongLongitude = 200;

        //Act

        //Asset
        await Assert.ThrowsAsync<GoogleApiException>(async () => await service.GetAddressFromCoordinates(wrongLatitude, wrongLongitude));
    }
}