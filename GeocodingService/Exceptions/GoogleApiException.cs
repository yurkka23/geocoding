namespace GeocodingService.Exceptions;

public class GoogleApiException : Exception
{
    public GoogleApiException(string message) : base(message) { }
}
