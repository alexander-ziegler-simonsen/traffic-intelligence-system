using System.Text;
using System.Text.Json;

namespace TisApi.Simulation;

public sealed class VehicleSimulator : IHostedService, IDisposable
{
    private sealed class VehicleState
    {
        public required SimRoute Route;
        public int CurrentStopIdx;  // the stop we are heading TOWARD (or dwelling at)
        public double Progress; // 0.0 → 1.0 along the current segment
        public bool Dwelling;
        public double DwellRemaining;   // seconds left to dwell
    }

    private readonly SimulatorState _state;
    private readonly IConfiguration _config;
    private readonly ILogger<VehicleSimulator> _logger;

    private List<VehicleState> _vehicles = [];
    private Timer? _timer;
    private int _ticking;   // interlocked flag — skip tick if previous one is still running

    public VehicleSimulator(SimulatorState state, IConfiguration config, ILogger<VehicleSimulator> logger)
    {
        _state = state;
        _config = config;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        try
        {
            var gtfsPath = _config["Simulation:GtfsPath"] ?? "../geodata/GTFS";
            _logger.LogInformation("Loading GTFS from {Path}", gtfsPath);

            var routes = await Task.Run(() => GtfsLoader.Load(gtfsPath), ct);
            _vehicles = CreateVehicles(routes);

            _logger.LogInformation("Vehicle simulator ready — {Count} vehicles across {Routes} routes",
                _vehicles.Count, routes.Count);

            _timer = new Timer(Tick, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vehicle simulator failed to start; /api/vehicles/geojson will return empty");
        }
    }

    public Task StopAsync(CancellationToken ct)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();

    // Tick

    private void Tick(object? _)
    {
        if (Interlocked.CompareExchange(ref _ticking, 1, 0) != 0) return;
        try
        {
            foreach (var v in _vehicles)
                Advance(v);

            _state.Update(BuildGeoJson());
        }
        finally
        {
            Interlocked.Exchange(ref _ticking, 0);
        }
    }

    private static void Advance(VehicleState v)
    {
        if (v.Dwelling)
        {
            v.DwellRemaining -= 1.0;
            if (v.DwellRemaining <= 0)
            {
                v.Dwelling = false;
                v.Progress = 0;
                v.CurrentStopIdx = (v.CurrentStopIdx + 1) % v.Route.Stops.Length;
            }
            return;
        }

        int fromIdx = (v.CurrentStopIdx - 1 + v.Route.Stops.Length) % v.Route.Stops.Length;
        double segDist = v.Route.SegmentDistances[fromIdx];

        v.Progress += v.Route.SpeedMs / segDist;

        if (v.Progress >= 1.0)
        {
            v.Progress = 1.0;
            v.Dwelling = true;
            v.DwellRemaining = v.Route.DwellSeconds;
        }
    }

    // GeoJSON

    private string BuildGeoJson()
    {
        using var ms = new MemoryStream(capacity: _vehicles.Count * 120);
        using var writer = new Utf8JsonWriter(ms);

        writer.WriteStartObject();
        writer.WriteString("type", "FeatureCollection");
        writer.WriteStartArray("features");

        foreach (var v in _vehicles)
        {
            (double lat, double lon) = GetPosition(v);

            writer.WriteStartObject();
            writer.WriteString("type", "Feature");

            writer.WriteStartObject("geometry");
            writer.WriteString("type", "Point");
            writer.WriteStartArray("coordinates");
            writer.WriteNumberValue(Math.Round(lon, 6));
            writer.WriteNumberValue(Math.Round(lat, 6));
            writer.WriteEndArray();
            writer.WriteEndObject();

            writer.WriteStartObject("properties");
            writer.WriteString("type", v.Route.VehicleType);
            writer.WriteString("route_id", v.Route.RouteId);
            writer.WriteString("route_name", v.Route.RouteName);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static (double Lat, double Lon) GetPosition(VehicleState v)
    {
        if (v.Dwelling)
        {
            var s = v.Route.Stops[v.CurrentStopIdx];
            return (s.Lat, s.Lon);
        }

        int fromIdx = (v.CurrentStopIdx - 1 + v.Route.Stops.Length) % v.Route.Stops.Length;
        var from = v.Route.Stops[fromIdx];
        var to = v.Route.Stops[v.CurrentStopIdx];

        return (
            from.Lat + (to.Lat - from.Lat) * v.Progress,
            from.Lon + (to.Lon - from.Lon) * v.Progress
        );
    }

    // Init

    private static List<VehicleState> CreateVehicles(List<SimRoute> routes)
    {
        var vehicles = new List<VehicleState>(routes.Count);

        foreach (var route in routes)
        {
            // Seeded per route so layout is deterministic across restarts
            var rng = new Random(route.RouteId.GetHashCode());

            vehicles.Add(new VehicleState
            {
                Route = route,
                CurrentStopIdx = rng.Next(1, route.Stops.Length),
                Progress = rng.NextDouble(),
                Dwelling = false,
                DwellRemaining = 0,
            });
        }
        return vehicles;
    }
}
