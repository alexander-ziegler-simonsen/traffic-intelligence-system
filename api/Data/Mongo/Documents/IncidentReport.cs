using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TisApi.Data.Mongo.Documents;

public class IncidentReport
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("postgres_id")]
    public int PostgresId { get; set; }

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("severity")]
    public int Severity { get; set; }

    [BsonElement("recorded_at")]
    public DateTime RecordedAt { get; set; }

    [BsonElement("camera")]
    public CameraInfo Camera { get; set; } = null!;

    [BsonElement("road")]
    public RoadInfo Road { get; set; } = null!;
}

public class CameraInfo
{
    [BsonElement("id")]
    public int Id { get; set; }

    [BsonElement("label")]
    public string Label { get; set; } = string.Empty;

    [BsonElement("latitude")]
    public double Latitude { get; set; }

    [BsonElement("longitude")]
    public double Longitude { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;
}

public class RoadInfo
{
    [BsonElement("id")]
    public int Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("speed_limit")]
    public int SpeedLimit { get; set; }

    [BsonElement("city")]
    public string City { get; set; } = string.Empty;
}
