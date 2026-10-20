using System.Net;
using System.Text.Json;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace api.Services;

// Asks the Valhalla routing service (the "valhalla" compose service) how a bus drives between stops
public class ValhallaClient
{
    private static readonly GeometryFactory Geometry4326 = NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    private readonly HttpClient _http;
    private readonly ILogger<ValhallaClient> _logger;

    public ValhallaClient(HttpClient http, ILogger<ValhallaClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    // The road a bus takes past the stops in order without entering the areas,
    // or null when Valhalla finds no way (e.g. a stop can only be reached through an area)
    public async Task<LineString?> RouteBusAsync(IReadOnlyList<Coordinate> stops, IReadOnlyList<Polygon> avoid, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            // "break" ends a leg there; in between, "break_through" stops without turning around
            locations = stops.Select((stop, i) => new
            {
                lat = stop.Y,
                lon = stop.X,
                type = i == 0 || i == stops.Count - 1 ? "break" : "break_through",
            }),
            costing = "bus",
            directions_type = "none",
            exclude_polygons = avoid.Select(area => area.ExteriorRing.Coordinates.Select(c => new[] { c.X, c.Y })),
        };

        using var response = await _http.PostAsJsonAsync("route", request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // e.g. 442 "No path could be found", 171 "No suitable edges near location", 167 area too big
            var error = await response.Content.ReadFromJsonAsync<ValhallaError>(cancellationToken);
            _logger.LogInformation("Valhalla found no bus route: {Code} {Error}", error?.ErrorCode, error?.Error);
            return null;
        }
        response.EnsureSuccessStatusCode();

        var route = await response.Content.ReadFromJsonAsync<ValhallaRoute>(cancellationToken);
        var coordinates = new List<Coordinate>();
        foreach (var leg in route?.Trip.Legs ?? [])
        {
            var points = DecodePolyline6(leg.Shape);
            // each leg starts where the one before it ended
            coordinates.AddRange(coordinates.Count > 0 ? points.Skip(1) : points);
        }

        return coordinates.Count >= 2 ? Geometry4326.CreateLineString(coordinates.ToArray()) : null;
    }

    // Valhalla sends shapes as Google encoded polylines with 6 decimals (lat, lon pairs)
    private static List<Coordinate> DecodePolyline6(string encoded)
    {
        var coordinates = new List<Coordinate>();
        int index = 0, lat = 0, lon = 0;
        while (index < encoded.Length)
        {
            lat += NextValue(encoded, ref index);
            lon += NextValue(encoded, ref index);
            coordinates.Add(new Coordinate(lon / 1e6, lat / 1e6));
        }
        return coordinates;
    }

    private static int NextValue(string encoded, ref int index)
    {
        int result = 0, shift = 0, chunk;
        do
        {
            chunk = encoded[index++] - 63;
            result |= (chunk & 0x1f) << shift;
            shift += 5;
        } while (chunk >= 0x20);
        return (result & 1) != 0 ? ~(result >> 1) : result >> 1;
    }

    private record ValhallaRoute(ValhallaTrip Trip);

    private record ValhallaTrip(List<ValhallaLeg> Legs);

    private record ValhallaLeg(string Shape);

    private record ValhallaError(
        [property: System.Text.Json.Serialization.JsonPropertyName("error_code")] int ErrorCode,
        string Error);
}
