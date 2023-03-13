using MongoDB.Bson.Serialization.Attributes;

namespace GeocodingService.Models;

public class CacheModel
{
    [BsonId]
    public string Address { get; set; }
    public string Coordinates { get; set; }

}
